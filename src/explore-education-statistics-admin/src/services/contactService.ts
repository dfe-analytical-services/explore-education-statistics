import client from '@admin/services/utils/service';

export interface ContactDetails {
  id: string;
  contactName: string;
  contactTelNo: string;
  teamEmail: string;
  teamName?: string;
}

const contactService = {
  getContacts(): Promise<ContactDetails[]> {
    return client.get<ContactDetails[]>('/contacts');
  },
};

export default contactService;
