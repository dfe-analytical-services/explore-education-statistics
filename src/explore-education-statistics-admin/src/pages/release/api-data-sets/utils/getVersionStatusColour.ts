import { DataSetVersionStatus } from '@admin/services/apiDataSetService';
import { TagProps } from '@common/components/Tag';

export default function getVersionStatusTagColour(
  status: DataSetVersionStatus,
): TagProps['colour'] {
  switch (status) {
    case 'Published':
      return 'blue';
    case 'Deprecated':
      return 'purple';
    case 'Withdrawn':
      return 'grey';
    case 'Draft':
      return 'green';
    case 'Cancelled':
    case 'Failed':
    case 'Mapping':
      return 'red';
    case 'Processing':
      return 'yellow';
    default:
      return 'grey';
  }
}
