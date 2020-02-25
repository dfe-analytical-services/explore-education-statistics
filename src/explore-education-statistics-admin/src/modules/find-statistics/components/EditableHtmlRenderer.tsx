import { ErrorControlContext } from '@admin/components/ErrorBoundary';
import WysiwygEditor from '@admin/components/WysiwygEditor';
import { RendererProps } from '@admin/modules/find-statistics/PublicationReleaseContent';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import wrapEditableComponent, {
  EditingContext,
} from '@common/modules/find-statistics/util/wrapEditableComponent';
import React, { useContext } from 'react';

export type Props = RendererProps & {
  source: string;
  canDelete: boolean;
  onDelete: () => void;
  editable?: boolean;
  onContentChange: (content: string) => Promise<unknown>;
};

const EditableHtmlRenderer = ({
  source = '',
  canDelete,
  onDelete,
  editable = true,
  onContentChange,
}: Props) => {
  return (
    <>
      <WysiwygEditor
        content={source}
        editable={editable}
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
