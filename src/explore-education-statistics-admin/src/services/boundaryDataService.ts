import client from '@admin/services/utils/service';
import { LocationLevelKey } from '@common/utils/locationLevelsMap';

export interface BoundaryLevel {
  id: string;
  level: LocationLevelKey;
  label: string;
  published: Date;
}

const boundaryDataService = {
  getBoundaryLevels(): Promise<BoundaryLevel[]> {
    return client.get('/boundary-level');
  },

  getBoundaryLevel(id: string): Promise<BoundaryLevel> {
    return client.get(`/boundary-level/${id}`);
  },

  updateBoundaryLevel(data: {
    id: string;
    label: string;
  }): Promise<BoundaryLevel> {
    return client.patch('/boundary-level', data);
  },

  uploadBoundaryFile(data: {
    level: string;
    label: string;
    published: Date;
    file: File;
  }): Promise<BoundaryLevel[]> {
    const formData = new FormData();
    formData.append('level', data.level);
    formData.append('label', data.label);
    formData.append('file', data.file);
    formData.append('published', data.published.toISOString());

    return client.post('/boundary-level', formData);
  },
};

export default boundaryDataService;
