import React from 'react';
import { TextRendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';

const EditableTextRenderer = ({ children }: TextRendererProps) => {
  return (
    <>
      {children}
    </>
  );
};

export default EditableTextRenderer;