import { DataSetVersionStatus } from '@admin/services/apiDataSetService';

export default function getDataSetVersionStatusText(
  status: DataSetVersionStatus,
): string {
  switch (status) {
    case 'Draft':
      return 'Ready';
    case 'Mapping':
      return 'Action required';
    default:
      return status;
  }
}
