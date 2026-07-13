import { ReportType } from '@patsanstha/reports-data-access';

export type ReportHubVisual = 'bars' | 'line' | 'columns' | 'progress' | 'ring' | 'grades';
export type ReportHubIconTone = 'blue' | 'orange' | 'red' | 'green';
export type ReportHubBadgeTone = 'success' | 'warning' | 'error';

export interface ReportHubCard {
  id: string;
  title: string;
  titleMr: string;
  description: string;
  descriptionMr: string;
  icon: string;
  iconTone: ReportHubIconTone;
  badge?: { label: string; tone: ReportHubBadgeTone };
  visual: ReportHubVisual;
  visualValue?: string;
  reportType?: ReportType;
  route?: string;
}

export const REPORT_HUB_CARDS: ReportHubCard[] = [
  {
    id: 'daily-collection',
    title: 'Daily Collection',
    titleMr: 'दैनिक वसुली',
    description: 'Track daily cash and digital collections across all branches.',
    descriptionMr: 'सर्व शाखांमधील दैनिक रोख आणि डिजिटल वसुली.',
    icon: 'description',
    iconTone: 'blue',
    badge: { label: 'UP-TO-DATE', tone: 'success' },
    visual: 'bars',
    reportType: ReportType.CollectionsDaily,
    route: '/reports/daily-collection',
  },
  {
    id: 'loan-outstanding',
    title: 'Loan Outstanding',
    titleMr: 'कर्ज बाकी',
    description: 'Monitor active loan balances and portfolio exposure.',
    descriptionMr: 'सक्रिय कर्ज शिल्लक आणि पोर्टफोलिओ मॉनिटर करा.',
    icon: 'account_balance_wallet',
    iconTone: 'blue',
    visual: 'line',
    reportType: ReportType.LoanPortfolio,
  },
  {
    id: 'deposit-report',
    title: 'Deposit Report',
    titleMr: 'ठेव अहवाल',
    description: 'Summary of savings, recurring, and fixed deposit accounts.',
    descriptionMr: 'बचत, आवर्ती आणि मुदत ठेव खात्यांचा सारांश.',
    icon: 'savings',
    iconTone: 'blue',
    visual: 'columns',
    route: '/deposits',
  },
  {
    id: 'overdue-report',
    title: 'Overdue Report',
    titleMr: 'थकबाकी अहवाल',
    description: 'Identify overdue EMIs and accounts requiring follow-up.',
    descriptionMr: 'थकबाकी EMI आणि पाठपुरावा आवश्यक खाती.',
    icon: 'notifications_active',
    iconTone: 'orange',
    badge: { label: 'CRITICAL', tone: 'warning' },
    visual: 'progress',
    visualValue: '65%',
    route: '/recovery',
  },
  {
    id: 'npa-report',
    title: 'NPA Report',
    titleMr: 'NPA अहवाल',
    description: 'Non-performing assets audit and classification summary.',
    descriptionMr: 'कामगिरी नसलेली मालमत्ता आणि वर्गीकरण सारांश.',
    icon: 'cancel',
    iconTone: 'red',
    badge: { label: 'NPA AUDIT', tone: 'error' },
    visual: 'ring',
    reportType: ReportType.NpaSummary,
  },
  {
    id: 'branch-performance',
    title: 'Branch Performance',
    titleMr: 'शाखा कामगिरी',
    description: 'Compare branch-wise collection and recovery performance.',
    descriptionMr: 'शाखानिहाय वसुली आणि कामगिरी तुलना.',
    icon: 'trending_up',
    iconTone: 'green',
    visual: 'grades',
    reportType: ReportType.BranchSummary,
  },
];
