import PublicationReleaseContent from '@admin/modules/find-statistics/PublicationReleaseContent';
import ManageReleaseContext, {
  ManageRelease,
} from '@admin/pages/release/ManageReleaseContext';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import FormFieldset from '@common/components/form/FormFieldset';
import FormRadioGroup from '@common/components/form/FormRadioGroup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import WarningMessage from '@common/components/WarningMessage';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import classNames from 'classnames';
import React, { useContext, useEffect, useState } from 'react';
import { getReleaseContent } from './helpers';
import {
  ReleaseProvider,
  useReleaseDispatch,
  useReleaseState,
} from './ReleaseContext';

type PageMode = 'edit' | 'preview';

const ReleaseContentPage = ({ handleApiErrors }: ErrorControlProps) => {
  const [pageMode, setPageMode] = useState<PageMode>('preview');

  const { releaseId, publication } = useContext(
    ManageReleaseContext,
  ) as ManageRelease;

  const { release, canUpdateRelease, unresolvedComments } = useReleaseState();
  const dispatch = useReleaseDispatch();

  useEffect(() => {
    getReleaseContent(dispatch, releaseId, handleApiErrors);
  }, [releaseId, publication.themeId, publication, handleApiErrors]);

  useEffect(() => {
    if (canUpdateRelease === true) {
      setPageMode('edit');
    }
  }, [canUpdateRelease]);

  return (
    <>
      {release ? (
        <>
          {canUpdateRelease && (
            <div className="govuk-form-group">
              {unresolvedComments.length > 0 && (
                <WarningMessage>
                  There are {unresolvedComments.length} unresolved comments
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
                  value={pageMode}
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
                    setPageMode(event.target.value as PageMode);
                  }}
                />
              </FormFieldset>
            </div>
          )}

          <div
            className={classNames('govuk-width-container', {
              'dfe-align--left-hand-controls': pageMode === 'edit',
              'dfe-hide-controls': pageMode === 'preview',
            })}
          >
            <div
              className={
                pageMode === 'edit' ? 'dfe-page-editing' : 'dfe-page-preview'
              }
            >
              <EditingContext.Provider
                value={{
                  isEditing: pageMode === 'edit',
                }}
              >
                <PublicationReleaseContent />
              </EditingContext.Provider>
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
