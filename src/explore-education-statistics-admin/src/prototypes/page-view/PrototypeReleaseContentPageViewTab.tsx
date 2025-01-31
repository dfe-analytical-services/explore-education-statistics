import {
  ReleaseContentProvider,
  useReleaseContentState,
} from '@admin/pages/release/content/contexts/ReleaseContentContext';
import { EditingContextProvider } from '@admin/contexts/EditingContext';
import PrototypeEditablePageModeToggle from '@admin/prototypes/page-view/PrototypeEditablePageModeToggle';
import prototypeReleaseContent from '@admin/prototypes/data/releaseContentData';
import styles from '@admin/prototypes/page-view/PrototypeReleaseContentPageViewTab.module.scss';
import classNames from 'classnames';
import React from 'react';
import PrototypeReleaseContent from './PrototypeReleaseContent';

const ReleaseContentPageLoaded = () => {
  const { canUpdateRelease, release } = useReleaseContentState();

  const canPreviewRelease = true;

  return (
    <EditingContextProvider editingMode={canUpdateRelease ? 'edit' : 'preview'}>
      {({ editingMode }) => {
        return (
          <>
            {canPreviewRelease && (
              <PrototypeEditablePageModeToggle
                canUpdateRelease={canUpdateRelease}
                previewLabel="Preview release page"
                showTablePreviewOption
              />
            )}
            <div
              className={classNames({
                [styles.container]: editingMode === 'edit',
                'govuk-width-container': editingMode !== 'table-preview',
              })}
            >
              <div
                className={
                  editingMode === 'edit'
                    ? 'dfe-page-editing'
                    : 'dfe-page-preview'
                }
              >
                {editingMode !== 'table-preview' && (
                  <>
                    <span className="govuk-caption-l">{release.title}</span>

                    <h2 className="govuk-heading-l dfe-print-break-before">
                      {release.publication.title}
                    </h2>

                    <PrototypeReleaseContent />
                  </>
                )}
                {editingMode === 'table-preview' && (
                  <p>Preview table tool here.</p>
                )}
              </div>
            </div>
          </>
        );
      }}
    </EditingContextProvider>
  );
};

const PrototypeReleaseContentPageViewTab = () => {
  const value = prototypeReleaseContent;
  return (
    <ReleaseContentProvider value={{ ...value, canUpdateRelease: true }}>
      <ReleaseContentPageLoaded />
    </ReleaseContentProvider>
  );
};

export default PrototypeReleaseContentPageViewTab;
