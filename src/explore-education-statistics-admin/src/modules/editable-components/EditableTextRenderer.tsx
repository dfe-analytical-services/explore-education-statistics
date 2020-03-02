import React from 'react';
import { FormTextInputProps } from '@common/components/form/FormTextInput';
import { RendererProps } from '../find-statistics/PublicationReleaseContent';

export type TextRendererProps = RendererProps & FormTextInputProps;

const EditableTextRenderer = ({ value }: TextRendererProps) => {
  return <>{value}</>;
};

export default EditableTextRenderer;
