import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';
import {
  ReportSnapshotDetail,
  ReportSnapshotStatus,
  ReportType,
} from '@patsanstha/reports-data-access';

/** Local PDF helper — avoids native-federation re-export issues from shared libs. */

function reportTypeLabel(type: ReportType): string {
  switch (type) {
    case ReportType.BranchSummary:
      return 'Branch Summary';
    case ReportType.LoanPortfolio:
      return 'Loan Portfolio';
    case ReportType.CollectionsDaily:
      return 'Collections Daily';
    case ReportType.NpaSummary:
      return 'NPA Summary';
    default:
      return 'Unknown';
  }
}

function reportStatusLabel(status: ReportSnapshotStatus): string {
  return ['Pending', 'Completed', 'Failed'][status] ?? 'Unknown';
}

function slugify(title: string): string {
  const slug = title
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '');

  return slug || 'report';
}

function formatLabel(name: string): string {
  return name
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, (char) => char.toUpperCase())
    .trim();
}

function formatScalar(value: unknown): string {
  if (value === null || value === undefined) return '—';
  if (typeof value === 'boolean') return value ? 'Yes' : 'No';
  if (typeof value === 'number') return String(value);
  if (typeof value === 'string') return value;
  return JSON.stringify(value);
}

function parseResultJson(resultJson: string): Record<string, unknown> {
  try {
    const parsed = JSON.parse(resultJson) as unknown;
    return typeof parsed === 'object' && parsed !== null && !Array.isArray(parsed)
      ? (parsed as Record<string, unknown>)
      : { result: parsed };
  } catch {
    return { result: resultJson };
  }
}

function isObjectArray(value: unknown): value is Record<string, unknown>[] {
  return Array.isArray(value) && value.every((item) => typeof item === 'object' && item !== null);
}

export function downloadReportPdf(report: ReportSnapshotDetail): void {
  const doc = new jsPDF({ unit: 'pt', format: 'a4' });
  const pageWidth = doc.internal.pageSize.getWidth();
  const margin = 40;
  let cursorY = margin;

  doc.setFont('helvetica', 'bold');
  doc.setFontSize(16);
  doc.setTextColor(26, 60, 110);
  doc.text('Patsanstha Credit Management', margin, cursorY);

  cursorY += 22;
  doc.setFontSize(11);
  doc.setTextColor(90, 90, 90);
  doc.text('Cooperative Credit Society Report', margin, cursorY);

  cursorY += 28;
  doc.setFontSize(20);
  doc.setTextColor(20, 20, 20);
  doc.text(report.title, margin, cursorY);

  cursorY += 22;
  doc.setFont('helvetica', 'normal');
  doc.setFontSize(11);
  doc.setTextColor(60, 60, 60);
  doc.text(`Type: ${reportTypeLabel(report.reportType)}`, margin, cursorY);
  cursorY += 16;
  doc.text(`Status: ${reportStatusLabel(report.status)}`, margin, cursorY);
  cursorY += 16;
  doc.text(`Generated: ${new Date(report.generatedAt).toLocaleString('en-IN')}`, margin, cursorY);

  cursorY += 24;
  doc.setDrawColor(220, 220, 220);
  doc.line(margin, cursorY, pageWidth - margin, cursorY);
  cursorY += 20;

  const result = parseResultJson(report.resultJson);
  const summaryRows: string[][] = [];
  const tableSections: Array<{ title: string; rows: Record<string, unknown>[] }> = [];

  for (const [key, value] of Object.entries(result)) {
    if (isObjectArray(value)) {
      tableSections.push({ title: formatLabel(key), rows: value });
      continue;
    }

    summaryRows.push([formatLabel(key), formatScalar(value)]);
  }

  if (summaryRows.length > 0) {
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(13);
    doc.setTextColor(26, 60, 110);
    doc.text('Summary', margin, cursorY);
    cursorY += 8;

    autoTable(doc, {
      startY: cursorY,
      head: [['Field', 'Value']],
      body: summaryRows,
      margin: { left: margin, right: margin },
      styles: { fontSize: 10, cellPadding: 6 },
      headStyles: { fillColor: [26, 60, 110], textColor: 255 },
      alternateRowStyles: { fillColor: [245, 247, 250] },
    });

    cursorY = (doc as jsPDF & { lastAutoTable?: { finalY: number } }).lastAutoTable?.finalY ?? cursorY;
    cursorY += 20;
  }

  for (const section of tableSections) {
    if (section.rows.length === 0) {
      continue;
    }

    if (cursorY > doc.internal.pageSize.getHeight() - 80) {
      doc.addPage();
      cursorY = margin;
    }

    doc.setFont('helvetica', 'bold');
    doc.setFontSize(13);
    doc.setTextColor(26, 60, 110);
    doc.text(section.title, margin, cursorY);
    cursorY += 8;

    const keys = Array.from(new Set(section.rows.flatMap((row) => Object.keys(row))));
    const headers = keys.map(formatLabel);
    const body = section.rows.map((row) => keys.map((key) => formatScalar(row[key])));

    autoTable(doc, {
      startY: cursorY,
      head: [headers],
      body,
      margin: { left: margin, right: margin },
      styles: { fontSize: 9, cellPadding: 5 },
      headStyles: { fillColor: [26, 60, 110], textColor: 255 },
      alternateRowStyles: { fillColor: [245, 247, 250] },
    });

    cursorY = (doc as jsPDF & { lastAutoTable?: { finalY: number } }).lastAutoTable?.finalY ?? cursorY;
    cursorY += 20;
  }

  if (summaryRows.length === 0 && tableSections.length === 0) {
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(11);
    doc.text('No report data available.', margin, cursorY);
  }

  const pageCount = doc.getNumberOfPages();
  for (let page = 1; page <= pageCount; page += 1) {
    doc.setPage(page);
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(9);
    doc.setTextColor(120, 120, 120);
    doc.text(
      `Page ${page} of ${pageCount}`,
      pageWidth - margin,
      doc.internal.pageSize.getHeight() - 20,
      { align: 'right' }
    );
  }

  const date = new Date(report.generatedAt).toISOString().slice(0, 10);
  doc.save(`${slugify(report.title)}-${date}.pdf`);
}
