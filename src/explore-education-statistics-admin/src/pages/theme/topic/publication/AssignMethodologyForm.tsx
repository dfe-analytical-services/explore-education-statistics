import { BasicMethodology, IdTitlePair } from '@admin/services/common/types';
import { ExternalMethodology } from '@admin/services/dashboard/types';
import service from '@admin/services/edit-publication/service';
import submitWithFormikValidation from '@admin/validation/formikSubmitHandler';
import withErrorControl, {
  ErrorControlProps,
} from '@admin/validation/withErrorControl';
import Button from '@common/components/Button';
import {
  FormFieldRadioGroup,
  FormFieldSelect,
  FormFieldTextInput,
  FormGroup,
  Formik,
} from '@common/components/form';
import { errorCodeToFieldError } from '@common/components/form/util/serverValidationHandler';
import Yup from '@common/lib/validation/yup';
import { Form, FormikProps } from 'formik';
import orderBy from 'lodash/orderBy';
import React, { useEffect, useState } from 'react';

export interface AssignMethodologyFormValues {
  methodologyChoice?: 'existing' | 'external' | 'later';
  selectedMethodologyId?: string;
  externalMethodology?: ExternalMethodology;
}

interface Props {
  methodology?: BasicMethodology;
  externalMethodology?: ExternalMethodology;
  publicationId: string;
  refreshPublication: () => void;
}

const AssignMethodologyForm = ({
  publicationId,
  methodology: currentMethodology,
  externalMethodology: currentExternalMethodology,
  handleApiErrors,
  refreshPublication,
}: Props & ErrorControlProps) => {
  const [formOpen, setFormOpen] = useState(false);
  const [methodologies, setMethodologies] = useState<IdTitlePair[]>();

  useEffect(() => {
    setFormOpen(false);
    service
      .getMethodologies()
      .then(setMethodologies)
      .catch(handleApiErrors);
  }, [currentMethodology, currentExternalMethodology, handleApiErrors]);

  const errorCodeMappings = [
    errorCodeToFieldError(
      'METHODOLOGY_DOES_NOT_EXIST',
      'methodologyChoice',
      'There was a problem adding the selected methodology',
    ),
    errorCodeToFieldError(
      'METHODOLOGY_MUST_BE_APPROVED_OR_PUBLISHED',
      'methodologyChoice',
      'Choose a methodology that is Live or ready to be published',
    ),
    errorCodeToFieldError(
      'METHODOLOGY_OR_EXTERNAL_METHODOLOGY_LINK_MUST_BE_DEFINED',
      'methodologyChoice',
      'Either an existing methodology or an external methodology link must be provided',
    ),
    errorCodeToFieldError(
      'CANNOT_SPECIFY_METHODOLOGY_AND_EXTERNAL_METHODOLOGY',
      'methodologyChoice',
      'Either an existing methodology or an external methodology link must be provided',
    ),
  ];

  const submitFormHandler = submitWithFormikValidation<
    AssignMethodologyFormValues
  >(
    async values => {
      const newMethodology = {
        externalMethodology:
          values.methodologyChoice === 'external'
            ? values.externalMethodology
            : undefined,
        selectedMethodologyId:
          values.methodologyChoice === 'existing'
            ? values.selectedMethodologyId
            : undefined,
      };
      await service.updatePublicationMethodology({
        publicationId,
        ...newMethodology,
      });
    },
    handleApiErrors,
    ...errorCodeMappings,
  );

  if (!formOpen)
    return (
      <>
        {currentMethodology ||
        (currentExternalMethodology && currentExternalMethodology.url) ? (
          <div>
            <strong>Current methodology:</strong>
            <br />{' '}
            {(currentMethodology && currentMethodology.title) ||
              (currentExternalMethodology && (
                <>
                  {currentExternalMethodology.title} (
                  <a href={currentExternalMethodology.url}>
                    {currentExternalMethodology.url}
                  </a>
                  )
                </>
              ))}
          </div>
        ) : (
          <div>This publication doesn't have a methodology.</div>
        )}
        <Button
          className="govuk-!-margin-top-6"
          onClick={() => setFormOpen(true)}
          disabled={!methodologies}
        >
          {!currentMethodology && !currentExternalMethodology
            ? 'Add'
            : 'Change'}{' '}
          methodology
        </Button>
      </>
    );
  const formId = 'assignMethodologyForm';
  return (
    <>
      <div className="govuk-!-margin-top-6">
        <Formik<AssignMethodologyFormValues>
          initialValues={{
            selectedMethodologyId:
              (currentMethodology && currentMethodology.id) || '',
            methodologyChoice: currentExternalMethodology
              ? 'external'
              : 'existing',
            externalMethodology: {
              title:
                (currentExternalMethodology &&
                  currentExternalMethodology.url &&
                  currentExternalMethodology.title) ||
                '',
              url:
                (currentExternalMethodology &&
                  currentExternalMethodology.url) ||
                'https://',
            },
          }}
          validationSchema={Yup.object<AssignMethodologyFormValues>({
            methodologyChoice: Yup.mixed().required('Choose a methodology'),
            selectedMethodologyId: Yup.string().when('methodologyChoice', {
              is: 'existing',
              then: Yup.string().required('Choose a methodology'),
              otherwise: Yup.string(),
            }),
            externalMethodology: Yup.object<{
              title: string;
              url: string;
            }>().when('methodologyChoice', {
              is: 'external',
              then: Yup.object().shape({
                title: Yup.string().required('Enter a link title'),
                url: Yup.string().url('Enter a valid URL'),
              }),
            }),
          })}
          onSubmit={submitFormHandler}
          render={(form: FormikProps<AssignMethodologyFormValues>) => (
            <Form id={formId}>
              <FormFieldRadioGroup
                id={`${formId}-methodologyChoice`}
                legend="Choose a methodology for this publication"
                legendSize="m"
                name="methodologyChoice"
                options={[
                  {
                    value: 'existing',
                    label: 'Choose an existing methodology',
                    conditional: (
                      <FormFieldSelect
                        id={`${formId}-selectedMethodologyId`}
                        name="selectedMethodologyId"
                        label="Select methodology"
                        options={(methodologies as (IdTitlePair & {
                          status: string;
                        })[]).map(methodology => ({
                          label: `${methodology.title} [${methodology.status}]`,
                          value: methodology.id,
                        }))}
                      />
                    ),
                  },
                  {
                    value: 'external',
                    label: 'Link to an externally hosted methodology',
                    conditional: (
                      <FormGroup>
                        <FormFieldTextInput
                          label="Link title"
                          id="externalMethodology.title"
                          name="externalMethodology.title"
                        />
                        <FormFieldTextInput
                          label="URL"
                          id="externalMethodology.url"
                          name="externalMethodology.url"
                        />
                      </FormGroup>
                    ),
                  },
                ]}
                onChange={e => {
                  if (e.target.value === 'existing') {
                    return form.setValues({
                      ...form.values,
                      selectedMethodologyId: orderBy(
                        methodologies,
                        methodology => methodology.title,
                      )[0].id,
                      externalMethodology: {
                        title: '',
                        url: '',
                      },
                    });
                  }
                  if (e.target.value === 'external') {
                    return form.setValues({
                      ...form.values,
                      selectedMethodologyId: '',
                    });
                  }
                  return form.setValues({
                    ...form.values,
                    selectedMethodologyId: '',
                    externalMethodology: {
                      title: '',
                      url: '',
                    },
                  });
                }}
              />
              <div className="govuk-!-margin-top-6">
                <Button className="govuk-!-margin-right-2" type="submit">
                  Update publication
                </Button>
                <Button variant="secondary" onClick={() => setFormOpen(false)}>
                  Cancel
                </Button>
              </div>
            </Form>
          )}
        />
      </div>
    </>
  );
};

export default withErrorControl(AssignMethodologyForm);
