import { StatusDetail } from '@admin/pages/release/hooks/useReleasePublishingStatus';

export default function getStatusDetail(status: string): StatusDetail {
  if (!status) {
    return { color: 'orange', text: 'Requesting status' };
  }
  switch (status) {
    case 'NotStarted':
      return { color: 'blue', text: 'Not Started' };
    case 'Scheduled':
      return { color: 'blue', text: status };
    case 'Failed':
    case 'Cancelled':
    case 'Superseded':
      return { color: 'red', text: status };
    case 'Validating':
    case 'Queued':
    case 'Started':
      return { color: 'orange', text: status };
    case 'Complete':
      return { color: 'green', text: status };
    default:
      return { color: 'red', text: 'Error' };
  }
}
