import PublicationReleaseContent from '@admin/modules/find-statistics/PublicationReleaseContent';
// eslint-disable-next-line import/no-named-as-default
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import releaseContentService from '@admin/services/release/edit-release/content/service';
import FormFieldset from '@common/components/form/FormFieldset';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import WarningMessage from '@common/components/WarningMessage';
import classNames from 'classnames';
import React, { useContext, useEffect, useState } from 'react';
import { ManageContentPageViewModel } from '@admin/services/release/edit-release/content/types';
import {
  EditableContentBlock,
  ExtendedComment,
} from '@admin/services/publicationService';
import { ContentSection } from '@common/services/publicationService';

type PageMode = 'edit' | 'preview';

interface Model {
  unresolvedComments: ExtendedComment[];
  pageMode: PageMode;
  content: ManageContentPageViewModel;
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

const ReleaseContentPage = () => {
  const [model, setModel] = useState<Model>();

  const { releaseId, publication } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  useEffect(() => {
    releaseContentService.getContent(releaseId).then(newContent => {
      setModel({
        unresolvedComments: getUnresolveComments(newContent.release),
        pageMode: 'edit',
        content: newContent,
      });
    });
  }, [releaseId, publication.themeId, publication]);

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
              />
            </div>
          </div>
        </>
      )}
    </>
  );
};

export default ReleaseContentPage;
