import createConnection from '@admin/services/hubs/utils/createConnection';
import Hub from './utils/Hub';

export class NotificationHub extends Hub {}

export default function notificationHub() {
  return new NotificationHub(createConnection('/hubs/service-announcement'));
}
