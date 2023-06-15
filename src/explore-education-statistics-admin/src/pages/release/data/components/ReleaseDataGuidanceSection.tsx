import { toolbarConfigs } from '@admin/config/ckEditorConfig';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import releaseDataGuidanceService from '@admin/services/releaseDataGuidanceService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';
import InsetText from '@common/components/InsetText';
import LoadingSpinner from '@common/components/LoadingSpinner';
import ContentHtml from '@common/components/ContentHtml';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useFormSubmit from '@common/hooks/useFormSubmit';
import useToggle from '@common/hooks/useToggle';
import ReleaseDataGuidanceDataFile from '@common/modules/release/components/ReleaseDataGuidanceDataFile';
import minDelay from '@common/utils/minDelay';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import toPath from 'lodash/toPath';
import React from 'react';

export interface DataGuidanceFormValues {
  content: string;
  subjects: {
    id: string;
    content: string;
  }[];
}

const initialReleaseDataGuidance = `
<h3>Description</h3>
<p>---</p>

<h3>Coverage</h3>
<p>---</p>

<h3>File formats and conventions</h3>
<p>---</p>
`;

interface Props {
  releaseId: string;
  canUpdateRelease: boolean;
}

const formId = 'dataGuidanceForm';

const ReleaseDataGuidanceSection = ({ releaseId, canUpdateRelease }: Props) => {
  const {
    value: dataGuidance,
    isLoading,
    setState: setDataGuidance,
  } = useAsyncHandledRetry(
    () => releaseDataGuidanceService.getDataGuidance(releaseId),
    [releaseId, canUpdateRelease],
  );

  const [isEditing, toggleEditing] = useToggle(canUpdateRelease);

  const handleSubmit = useFormSubmit<DataGuidanceFormValues>(
    async (values, helpers) => {
      await minDelay(async () => {
        const updatedGuidance =
          await releaseDataGuidanceService.updateDataGuidance(
            releaseId,
            values,
          );

        setDataGuidance({ value: updatedGuidance });

        helpers.resetForm();
        toggleEditing.off();
      }, 600);
    },
  );

  return (
    <>
      <h2>Public data guidance</h2>

      {canUpdateRelease ? (
        <InsetText>
          <h3>Before you start</h3>

          <ul>
            <li>
              upload at least one data file before creating the data guidance
            </li>
            <li>
              ensure all data guidance has been populated and is up-to-date
              before seeking release approval
            </li>
            <li>
              this document will become public facing when the publication is
              released
            </li>
          </ul>
        </InsetText>
      ) : (
        <WarningMessage>
          This release has been approved, and can no longer be updated.
        </WarningMessage>
      )}

      <LoadingSpinner loading={isLoading}>
        {dataGuidance && (
          // eslint-disable-next-line react/jsx-no-useless-fragment
          <>
            {dataGuidance.subjects.length > 0 ? (
              <Formik<DataGuidanceFormValues>
                enableReinitialize
                initialValues={{
                  content: dataGuidance.content || initialReleaseDataGuidance,
                  subjects: dataGuidance.subjects.map(subject => ({
                    id: subject.id,
                    content: subject.content,
                  })),
                }}
                validationSchema={Yup.object<DataGuidanceFormValues>({
                  content: Yup.string().required('Enter main guidance content'),
                  subjects: Yup.array().of(
                    Yup.object({
                      id: Yup.string(),
                      content: Yup.string().required(params => {
                        const [, index] = toPath(params.path);
                        const subject = dataGuidance?.subjects[Number(index)];

                        if (!subject) {
                          return null;
                        }

                        return `Enter file guidance content for ${subject.name}`;
                      }),
                    }),
                  ),
                })}
                onSubmit={handleSubmit}
              >
                {form => {
                  const hasSubmitValidationError =
                    form.submitCount > 0 && !form.isValid ? true : undefined;

                  return (
                    <Form id={formId}>
                      {isEditing ? (
                        <FormFieldEditor<DataGuidanceFormValues>
                          name="content"
                          label="Main guidance content"
                        />
                      ) : (
                        // eslint-disable-next-line react/jsx-no-useless-fragment
                        <>
                          {!canUpdateRelease && !dataGuidance?.content ? (
                            <InsetText>
                              No guidance content was saved.
                            </InsetText>
                          ) : (
                            <ContentHtml
                              html={form.values.content}
                              testId="mainGuidanceContent"
                            />
                          )}
                        </>
                      )}

                      {dataGuidance.subjects.length > 0 && (
                        <>
                          <h3 className="govuk-!-margin-top-6">Data files</h3>

                          <Accordion
                            id="dataGuidance-dataFiles"
                            openAll={hasSubmitValidationError || isEditing}
                          >
                            {dataGuidance.subjects.map((subject, index) => (
                              <AccordionSection
                                heading={subject.name}
                                key={subject.id}
                              >
                                <ReleaseDataGuidanceDataFile
                                  key={subject.id}
                                  subject={subject}
                                  renderContent={() =>
                                    isEditing ? (
                                      <FormFieldEditor<DataGuidanceFormValues>
                                        toolbarConfig={toolbarConfigs.simple}
                                        name={`subjects[${index}].content`}
                                        label="File guidance content"
                                        testId="fileGuidanceContent"
                                      />
                                    ) : (
                                      <ContentHtml
                                        html={
                                          form.values.subjects[index].content
                                        }
                                        testId="fileGuidanceContent"
                                      />
                                    )
                                  }
                                />
                              </AccordionSection>
                            ))}
                          </Accordion>
                        </>
                      )}

                      {canUpdateRelease && (
                        <ButtonGroup>
                          {isEditing && (
                            <Button type="submit" disabled={form.isSubmitting}>
                              Save guidance
                            </Button>
                          )}

                          <Button
                            variant={isEditing ? 'secondary' : undefined}
                            onClick={toggleEditing}
                          >
                            {isEditing ? 'Preview guidance' : 'Edit guidance'}
                          </Button>

                          {isEditing && (
                            <ButtonText onClick={() => form.resetForm()}>
                              Cancel
                            </ButtonText>
                          )}
                        </ButtonGroup>
                      )}
                    </Form>
                  );
                }}
              </Formik>
            ) : (
              // eslint-disable-next-line react/jsx-no-useless-fragment
              <>
                {canUpdateRelease ? (
                  <WarningMessage>
                    Before you can change the public data guidance, you must
                    upload at least one data file.
                  </WarningMessage>
                ) : (
                  <InsetText>
                    The public data guidance has not been created as no data
                    files were uploaded.
                  </InsetText>
                )}
              </>
            )}
          </>
        )}
      </LoadingSpinner>
    </>
  );
};

export default ReleaseDataGuidanceSection;
