import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import { FormTextInputProps } from '@common/components/form/FormTextInput';
import React from 'react';

export type TextRendererProps = RendererProps & FormTextInputProps;

const EditableTextRenderer = ({ value }: TextRendererProps) => {
  return <>{value}</>;
};

export default EditableTextRenderer;
