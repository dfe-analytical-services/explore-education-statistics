import client from '@admin/services/utils/service';

export interface BoundaryLevel {
  id: string;
  level: string;
  label: string;
  published: Date;
}

const boundaryDataService = {
  getBoundaryLevels(): Promise<BoundaryLevel[]> {
    return client.get('/boundary-data/levels');
  },

  getBoundaryLevel(id: string): Promise<BoundaryLevel> {
    return client.get(`/boundary-data/levels/${id}`);
  },

  updateBoundaryLevel(data: {
    id: string;
    label: string;
  }): Promise<BoundaryLevel> {
    return client.patch('/boundary-data/levels', data);
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

    return client.post('/boundary-data', formData);
  },
};

export default boundaryDataService;
