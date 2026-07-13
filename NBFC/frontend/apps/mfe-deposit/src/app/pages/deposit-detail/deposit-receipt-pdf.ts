import { jsPDF } from 'jspdf';
import { DepositAccountDetail, formatInr } from '@patsanstha/deposits-data-access';

export interface DepositTransactionReceipt {
  id: string;
  date: string;
  description: string;
  descriptionMr: string;
  type: 'CREDIT' | 'DEBIT';
  amount: number;
}

/** Local PDF helper — avoids native-federation re-export issues. */
export function downloadDepositTransactionPdf(
  account: DepositAccountDetail,
  txn: DepositTransactionReceipt
): void {
  const doc = new jsPDF({ unit: 'pt', format: 'a4' });
  const margin = 48;
  let y = margin;

  doc.setFont('helvetica', 'bold');
  doc.setFontSize(16);
  doc.setTextColor(26, 60, 110);
  doc.text('Patsanstha Credit Management', margin, y);

  y += 20;
  doc.setFontSize(11);
  doc.setTextColor(90, 90, 90);
  doc.setFont('helvetica', 'normal');
  doc.text('Deposit Transaction Receipt', margin, y);

  y += 28;
  doc.setDrawColor(220, 220, 220);
  doc.line(margin, y, 547, y);
  y += 28;

  doc.setFont('helvetica', 'bold');
  doc.setFontSize(18);
  doc.setTextColor(20, 20, 20);
  doc.text(txn.description, margin, y);

  y += 18;
  doc.setFont('helvetica', 'normal');
  doc.setFontSize(11);
  doc.setTextColor(90, 90, 90);
  doc.text(txn.descriptionMr, margin, y);

  y += 32;
  const rows: Array<[string, string]> = [
    ['Account Number', account.accountNumber],
    ['Member', `${account.memberName} (${account.memberNumber})`],
    ['Date', txn.date],
    ['Type', txn.type],
    ['Amount', `${txn.type === 'CREDIT' ? '+' : '-'} ${formatInr(txn.amount)}`],
    ['Current Balance', formatInr(account.currentBalance)],
    ['Interest Rate', `${account.interestRate}% p.a.`],
  ];

  doc.setFontSize(12);
  for (const [label, value] of rows) {
    doc.setFont('helvetica', 'bold');
    doc.setTextColor(90, 90, 90);
    doc.text(label, margin, y);
    doc.setFont('helvetica', 'normal');
    doc.setTextColor(20, 20, 20);
    doc.text(value, margin + 160, y);
    y += 22;
  }

  y += 20;
  doc.setDrawColor(220, 220, 220);
  doc.line(margin, y, 547, y);
  y += 24;

  doc.setFontSize(10);
  doc.setTextColor(120, 120, 120);
  doc.text('This is a system-generated receipt from Patsanstha Credit Management.', margin, y);

  const safeAccount = account.accountNumber.replace(/[^a-zA-Z0-9-]/g, '');
  doc.save(`deposit-receipt-${safeAccount}-${txn.id}.pdf`);
}
