import PublicationReleaseContent from '@admin/modules/find-statistics/PublicationReleaseContent';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import {
  EditableContentBlock,
  ExtendedComment,
} from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import FormFieldset from '@common/components/form/FormFieldset';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import WarningMessage from '@common/components/WarningMessage';
import { ContentSection } from '@common/services/publicationService';
import classNames from 'classnames';
import React, { useContext, useEffect, useState } from 'react';
import { DataBlock } from '@common/services/dataBlockService';

type PageMode = 'edit' | 'preview';

interface Model {
  unresolvedComments: ExtendedComment[];
  pageMode: PageMode;
  content: ManageContentPageViewModel;
  availableDataBlocks: DataBlock[];
}

const contentSectionComments = (
  contentSection?: ContentSection<EditableContentBlock>,
) => {
  if (
    contentSection &&
    contentSection.content &&
    contentSection.content.length > 0
  ) {
    return contentSection.content.reduce<ExtendedComment[]>(
      (allCommentsForSection, content) =>
        content.comments
          ? [...allCommentsForSection, ...content.comments]
          : allCommentsForSection,
      [],
    );
  }

  return [];
};

const getUnresolveComments = (release: ManageContentPageViewModel['release']) =>
  [
    ...contentSectionComments(release.summarySection),
    ...contentSectionComments(release.keyStatisticsSection),
    ...release.content
      .filter(_ => _.content !== undefined)
      .reduce<ExtendedComment[]>(
        (allComments, contentSection) => [
          ...allComments,
          ...contentSectionComments(contentSection),
        ],
        [],
      ),
  ].filter(comment => comment !== undefined && comment.state === 'open');

const ReleaseContentPage = ({ handleApiErrors }: ErrorControlProps) => {
  const [model, setModel] = useState<Model>();

  const { releaseId, publication } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  useEffect(() => {
    releaseContentService
      .getContent(releaseId)
      .then(newContent => {
        // TODO: For testing purposes only
        if (
          newContent.release.summarySection &&
          newContent.release.summarySection.content &&
          newContent.release.summarySection.content.length
        ) {
          // eslint-disable-next-line no-param-reassign
          newContent.release.summarySection.content[0].comments = [
            {
              time: new Date(),
              name: 'Jamie',
              state: 'open',
              comment: 'test',
              id: '00000000',
            },
          ];
        }

        setModel({
          unresolvedComments: getUnresolveComments(newContent.release),
          pageMode: 'edit',
          content: newContent,
          availableDataBlocks: newContent.availableDataBlocks,
        });
      })
      .catch(handleApiErrors);
  }, [releaseId, publication.themeId, publication, handleApiErrors]);

  const onReleaseChange = React.useCallback(
    (newRelease: ManageContentPageViewModel['release']) => {
      if (model) {
        setModel({
          ...model,
          unresolvedComments: getUnresolveComments(newRelease),
        });
      }
    },
    [model],
  );

  return (
    <>
      {model && (
        <>
          <div className="govuk-form-group">
            {model.unresolvedComments.length > 0 && (
              <WarningMessage>
                There are {model.unresolvedComments.length} unresolved comments
              </WarningMessage>
            )}

            <FormFieldset
              id="pageModelFieldset"
              legend=""
              className="dfe-toggle-edit"
              legendHidden
            >
              <FormRadioGroup
                id="pageMode"
                name="pageMode"
                value={model.pageMode}
                legend="Set page view"
                options={[
                  {
                    label: 'Add / view comments and edit content',
                    value: 'edit',
                  },
                  {
                    label: 'Preview content',
                    value: 'preview',
                  },
                ]}
                inline
                onChange={event => {
                  setModel({
                    ...model,
                    pageMode: event.target.value as PageMode,
                  });
                }}
              />
            </FormFieldset>
          </div>
          <div
            className={classNames('govuk-width-container', {
              'dfe-align--comments': model.pageMode === 'edit',
              'dfe-hide-comments': model.pageMode === 'preview',
            })}
          >
            <div className={model.pageMode === 'edit' ? 'page-editing' : ''}>
              <PublicationReleaseContent
                editing={model.pageMode === 'edit'}
                content={model.content}
                styles={{}}
                onReleaseChange={c => onReleaseChange(c)}
                availableDataBlocks={model.availableDataBlocks}
              />
            </div>
          </div>
        </>
      )}
    </>
  );
};

export default withErrorControl(ReleaseContentPage);
