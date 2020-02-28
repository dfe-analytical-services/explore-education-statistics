import PublicationReleaseContent from '@admin/modules/find-statistics/PublicationReleaseContent';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import {
  EditableContentBlock,
  ExtendedComment,
} from '@admin/services/publicationService';
import { releaseContentService } from '@admin/services/release/edit-release/content/service';
import permissionService from '@admin/services/permissions/service';
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
import LoadingSpinner from '@common/components/LoadingSpinner';
import { ReleaseProvider } from './ReleaseContext';

type PageMode = 'edit' | 'preview';

interface Model {
  unresolvedComments: ExtendedComment[];
  pageMode: PageMode;
  release: ManageContentPageViewModel['release'];
  availableDataBlocks: DataBlock[];
  canUpdateRelease: boolean;
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
    Promise.all([
      releaseContentService.getContent(releaseId),
      permissionService.canUpdateRelease(releaseId),
    ])
      .then(([{ release, availableDataBlocks }, canUpdateRelease]) => {
        setModel({
          unresolvedComments: getUnresolveComments(release),
          pageMode: canUpdateRelease ? 'edit' : 'preview',
          release,
          availableDataBlocks,
          canUpdateRelease,
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
      {model ? (
        <>
          {model.canUpdateRelease && (
            <div className="govuk-form-group">
              {model.unresolvedComments.length > 0 && (
                <WarningMessage>
                  There are {model.unresolvedComments.length} unresolved
                  comments
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
                  small
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
                  onChange={event => {
                    setModel({
                      ...model,
                      pageMode: event.target.value as PageMode,
                    });
                  }}
                />
              </FormFieldset>
            </div>
          )}

          <div
            className={classNames('govuk-width-container', {
              'dfe-align--left-hand-controls': model.pageMode === 'edit',
              'dfe-hide-controls': model.pageMode === 'preview',
            })}
          >
            <div
              className={
                model.pageMode === 'edit'
                  ? 'dfe-page-editing'
                  : 'dfe-page-preview'
              }
            >
              <PublicationReleaseContent
                editing={model.pageMode === 'edit'}
                release={model.release}
                styles={{}}
                onReleaseChange={c => onReleaseChange(c)}
                availableDataBlocks={model.availableDataBlocks}
              />
            </div>
          </div>
        </>
      ) : (
        <LoadingSpinner />
      )}
    </>
  );
};

export default withErrorControl(props => (
  <ReleaseProvider>
    <ReleaseContentPage {...props} />
  </ReleaseProvider>
));
