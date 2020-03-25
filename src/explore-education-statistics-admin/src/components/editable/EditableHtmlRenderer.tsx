import WysiwygEditor from '@admin/components/WysiwygEditor';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import wrapEditableComponent from '@common/modules/find-statistics/util/wrapEditableComponent';
import React from 'react';

export type Props = RendererProps & {
  source: string;
  canDelete: boolean;
  onDelete: () => void;
  editable?: boolean;
  insideAccordion?: boolean;
  onContentChange: (content: string) => void;
};

const EditableHtmlRenderer = ({
  source = '',
  canDelete,
  insideAccordion,
  onDelete,
  editable = true,
  onContentChange,
}: Props) => {
  return (
    <>
      <WysiwygEditor
        content={source}
        editable={editable}
        insideAccordion={insideAccordion}
        canDelete={canDelete}
        onContentChange={onContentChange}
        onDelete={onDelete}
      />
    </>
  );
};

const HtmlRenderer = ({ source }: { source: string }) => {
  return (
    <div
      // eslint-disable-next-line react/no-danger
      dangerouslySetInnerHTML={{ __html: source }}
    />
  );
};

export default wrapEditableComponent(EditableHtmlRenderer, HtmlRenderer);
