// eslint-disable-next-line import/no-named-as-default
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import commonService from '@admin/services/common/service';
import { IdTitlePair } from '@admin/services/common/types';
import { Comment } from '@admin/services/dashboard/types';
import releaseContentService from '@admin/services/release/edit-release/content/service';
import releaseSummaryService from '@admin/services/release/edit-release/summary/service';
import { ReleaseSummaryDetails } from '@admin/services/release/types';
import FormFieldset from '@common/components/form/FormFieldset';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import WarningMessage from '@common/components/WarningMessage';
import { AbstractRelease } from '@common/services/publicationService';
import classNames from 'classnames';
import React, { useContext, useEffect, useState } from 'react';
import PublicationReleaseContent from '@admin/modules/find-statistics/PublicationReleaseContent';
import { EditableContentBlock } from '@admin/services/publicationService';

type PageMode = 'edit' | 'preview';

interface Model {
  unresolvedComments: Comment[];
  pageMode: PageMode;
  releaseSummary: ReleaseSummaryDetails;
  theme: IdTitlePair;
  release: AbstractRelease<EditableContentBlock>;
}

const ReleaseContentPage = () => {
  const [model, setModel] = useState<Model>();

  const { releaseId, publication } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  useEffect(() => {
    Promise.all([
      releaseSummaryService.getReleaseSummaryDetails(releaseId),
      commonService.getBasicThemeDetails(publication.themeId),
      releaseContentService.getRelease(releaseId),
      releaseContentService.getContentSections(releaseId),
    ]).then(([releaseSummary, theme, releaseData, releaseContentSections]) => {
      // <editor-fold desc="TODO - content population">

      const unresolvedComments: Comment[] = [
        {
          message: 'Please resolve this.\nThank you.',
          authorName: 'Amy Newton',
          createdDate: new Date('2019-08-10 10:15').toISOString(),
        },
        {
          message: 'And this too.\nThank you.',
          authorName: 'Dave Matthews',
          createdDate: new Date('2019-06-13 10:15').toISOString(),
        },
      ];

      const releaseDataAsEditable = {
        ...releaseData,
        keyStatistics: releaseData.keyStatistics as EditableContentBlock,
        content: releaseContentSections,
      };

      const release: AbstractRelease<EditableContentBlock> = {
        ...releaseDataAsEditable,
        summary: 'This is the summary ..... ',
        updates: [
          {
            id: '',
            on: '',
            reason: '',
            releaseId: '',
          },
        ],
        publication: {
          ...publication,
          slug: '',
          description: '',
          dataSource: '',
          summary: '',
          releases: [],
          legacyReleases: [],
          topic: {
            theme,
          },
          nextUpdate: '',
          contact: {
            contactName: '',
            contactTelNo: '',
            teamEmail: '',
            teamName: '',
          },
        },
      };

      // </editor-fold>

      setModel({
        unresolvedComments,
        pageMode: 'edit',
        releaseSummary,
        theme,
        release,
      });
    });
  }, [releaseId, publication.themeId, publication]);

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
                basicPublication={publication}
                release={model.release}
                releaseSummary={model.releaseSummary}
                styles={{}}
              />
            </div>
          </div>
        </>
      )}
    </>
  );
};

export default ReleaseContentPage;
