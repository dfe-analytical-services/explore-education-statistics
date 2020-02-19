import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import { FormFieldset, FormRadioGroup } from '@common/components/form';
import { EditingContext } from '@common/modules/find-statistics/util/wrapEditableComponent';
import React, { useState } from 'react';
import { MethodologyTabProps } from '../MethodologyPage';

type PageMode = 'edit' | 'preview';

const MethodologyContentPage = ({
  methodology,
}: ErrorControlProps & MethodologyTabProps) => {
  const [pageMode, setPageMode] = useState<PageMode>('edit');

  return (
    <>
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
              label: 'Add / edit methodology content',
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
      <EditingContext.Provider
        value={{
          isEditing: pageMode === 'edit',
          isCommenting: false,
          isReviewing: false,
          releaseId: methodology.id,
          availableDataBlocks: [],
        }}
      >
        <main
          className="govuk-main-wrapper app-main-class"
          id="main-content"
          role="main"
        >
          <h1
            className="govuk-heading-xl"
            data-testid={`page-title ${methodology.title}`}
          >
            {methodology.title}
          </h1>
          <p>Lorem ipsum.</p>
          <p>{JSON.stringify(methodology)}</p>
        </main>
      </EditingContext.Provider>
    </>
  );
};

export default withErrorControl(MethodologyContentPage);
