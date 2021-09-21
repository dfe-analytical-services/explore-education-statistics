import client from '@admin/services/utils/service';

export interface MethodologyNote {
  id: string;
  content: string;
  displayDate: Date;
}

const methodologyNoteService = {
  create(
    methodologyId: string,
    methodologyNote: Omit<MethodologyNote, 'id'>,
  ): Promise<MethodologyNote> {
    return client.post(
      `/methodologies/${methodologyId}/notes`,
      methodologyNote,
    );
  },

  edit(
    noteId: string,
    methodologyId: string,
    methodologyNote: Omit<MethodologyNote, 'id'>,
  ): Promise<MethodologyNote> {
    return client.put(
      `/methodologies/${methodologyId}/notes/${noteId}`,
      methodologyNote,
    );
  },

  delete(noteId: string, methodologyId: string): Promise<MethodologyNote[]> {
    return client.delete(`/methodologies/${methodologyId}/notes/${noteId}`);
  },
};

export default methodologyNoteService;
