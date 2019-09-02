import {Comment} from "@admin/services/dashboard/types";
import FormFieldset from "@common/components/form/FormFieldset";
import FormRadio from "@common/components/form/FormRadio";
import FormRadioGroup from "@common/components/form/FormRadioGroup";
import React, {useEffect, useState} from 'react';

interface Model {
  unresolvedComments: Comment[];
  pageMode: 'edit' | 'preview';
}

const ReleaseContentPage = () => {

  const [model, setModel] = useState();

  useEffect(() => {
    const unresolvedComments: Comment[] = [{
      message: 'Please resolve this.\nThank you.',
      authorName: 'Amy Newton',
      createdDate: new Date('2019-08-10 10:15').toISOString(),
    }, {
      message: 'And this too.\nThank you.',
      authorName: 'Dave Matthews',
      createdDate: new Date('2019-08-10 10:15').toISOString(),
    }];

    setModel({
      unresolvedComments,
      pageView: 'edit',
    })
  }, []);

  return (
    <>
      {model && (
        <div className="govuk-form-group">
          {model.unresolvedComments.length > 0 && (
            <div className="govuk-warning-text">
              <span className="govuk-warning-text__icon" aria-hidden="true">
                !
              </span>
              <strong className="govuk-warning-text__text">
                <span className="govuk-warning-text__assistive">Warning</span>
                There are {model.unresolvedComments.length} unresolved comments
              </strong>
            </div>
          )}

          <FormFieldset
            id='pageModelFieldSet'
            legend=''
            className='dfe-toggle-edit'
            legendHidden
          >
            <FormRadioGroup
              id='pageMode'
              name='pageMode'
              value={model.pageMode}
              legend='Set page view'
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
            />
          </FormFieldset>
        </div>
      )}
    </>
  )
};

export default ReleaseContentPage;
