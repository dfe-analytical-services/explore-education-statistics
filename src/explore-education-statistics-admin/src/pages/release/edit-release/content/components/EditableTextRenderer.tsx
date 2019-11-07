import React from 'react';
import { TextRendererProps } from '@common/modules/find-statistics/PublicationReleaseContent';

const EditableTextRenderer = ({ children }: TextRendererProps) => {
  return (
    <>
      {children}
    </>
  );
};

export default EditableTextRenderer;