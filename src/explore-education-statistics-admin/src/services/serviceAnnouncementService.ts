import client from '@admin/services/utils/service';

const serviceAnnouncementService = {
  broadcastMessage(data: {
    message: string;
    connectionId: string;
  }): Promise<void> {
    const formData = new FormData();
    formData.append('message', data.message);
    formData.append('connectionId', data.connectionId);

    return client.post('/broadcastMessage', formData);
  },
};

export default serviceAnnouncementService;
