import { toolbarConfigs } from '@admin/components/form/FormEditor';
import FormFieldEditor from '@admin/components/form/FormFieldEditor';
import useFormSubmit from '@admin/hooks/useFormSubmit';
import releaseMetaGuidanceService from '@admin/services/releaseMetaGuidanceService';
import Accordion from '@common/components/Accordion';
import AccordionSection from '@common/components/AccordionSection';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import { Form } from '@common/components/form';
import LoadingSpinner from '@common/components/LoadingSpinner';
import SanitizeHtml from '@common/components/SanitizeHtml';
import WarningMessage from '@common/components/WarningMessage';
import useAsyncHandledRetry from '@common/hooks/useAsyncHandledRetry';
import useToggle from '@common/hooks/useToggle';
import ReleaseMetaGuidanceDataFile from '@common/modules/release/components/ReleaseMetaGuidanceDataFile';
import minDelay from '@common/utils/minDelay';
import Yup from '@common/validation/yup';
import { Formik } from 'formik';
import toPath from 'lodash/toPath';
import React from 'react';

export interface MetaGuidanceFormValues {
  content: string;
  subjects: {
    id: string;
    content: string;
  }[];
}

const initialReleaseMetaGuidance = `
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

const formId = 'metaGuidanceForm';

const ReleaseMetaGuidanceSection = ({ releaseId, canUpdateRelease }: Props) => {
  const {
    value: metaGuidance,
    isLoading,
    setState: setMetaGuidance,
  } = useAsyncHandledRetry(
    () => releaseMetaGuidanceService.getMetaGuidance(releaseId),
    [releaseId, canUpdateRelease],
  );

  const [isEditing, toggleEditing] = useToggle(canUpdateRelease);

  const handleSubmit = useFormSubmit<MetaGuidanceFormValues>(
    async (values, helpers) => {
      await minDelay(async () => {
        const updatedGuidance = await releaseMetaGuidanceService.updateMetaGuidance(
          releaseId,
          values,
        );

        setMetaGuidance({ value: updatedGuidance });

        helpers.resetForm();
        toggleEditing.off();
      }, 600);
    },
  );

  return (
    <>
      <h2>Public metadata guidance document</h2>

      {canUpdateRelease ? (
        <div className="govuk-inset-text">
          <h3>Before you start</h3>

          <ul>
            <li>
              upload at least one data file before creating the metadata
              guidance document
            </li>
            <li>
              ensure all metadata guidance has been populated and is up-to-date
              before seeking release approval
            </li>
            <li>
              this document will become public facing when the publication is
              released
            </li>
          </ul>
        </div>
      ) : (
        <WarningMessage>
          This release has been approved, and can no longer be updated.
        </WarningMessage>
      )}

      <LoadingSpinner loading={isLoading}>
        {metaGuidance && (
          <>
            {metaGuidance.subjects.length > 0 ? (
              <Formik<MetaGuidanceFormValues>
                enableReinitialize
                initialValues={{
                  content: metaGuidance.content || initialReleaseMetaGuidance,
                  subjects: metaGuidance.subjects.map(subject => ({
                    id: subject.id,
                    content: subject.content,
                  })),
                }}
                validationSchema={Yup.object<MetaGuidanceFormValues>({
                  content: Yup.string().required('Enter main guidance content'),
                  subjects: Yup.array().of(
                    Yup.object({
                      id: Yup.string(),
                      content: Yup.string().required(params => {
                        const [, index] = toPath(params.path);
                        const subject = metaGuidance?.subjects[Number(index)];

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
                        <FormFieldEditor<MetaGuidanceFormValues>
                          id={`${formId}-content`}
                          name="content"
                          label="Main guidance content"
                        />
                      ) : (
                        <>
                          {!canUpdateRelease && !metaGuidance?.content ? (
                            <p className="govuk-inset-text">
                              No guidance content was saved.
                            </p>
                          ) : (
                            <SanitizeHtml
                              dirtyHtml={form.values.content}
                              testId="mainGuidanceContent"
                            />
                          )}
                        </>
                      )}

                      {metaGuidance.subjects.length > 0 && (
                        <>
                          <h3 className="govuk-!-margin-top-6">Data files</h3>

                          <Accordion
                            id="metaGuidance-dataFiles"
                            openAll={hasSubmitValidationError || isEditing}
                          >
                            {metaGuidance.subjects.map((subject, index) => (
                              <AccordionSection
                                heading={subject.name}
                                key={subject.id}
                              >
                                <ReleaseMetaGuidanceDataFile
                                  key={subject.id}
                                  subject={subject}
                                  renderContent={() =>
                                    isEditing ? (
                                      <FormFieldEditor<MetaGuidanceFormValues>
                                        toolbarConfig={toolbarConfigs.simple}
                                        id={`${formId}-subjects${index}Content`}
                                        name={`subjects[${index}].content`}
                                        label="File guidance content"
                                      />
                                    ) : (
                                      <SanitizeHtml
                                        dirtyHtml={
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
              <>
                {canUpdateRelease ? (
                  <WarningMessage>
                    Before you can change the public metadata guidance, you must
                    upload at least one data file.
                  </WarningMessage>
                ) : (
                  <p className="govuk-inset-text">
                    The public metadata guidance document has not been created
                    as no data files were uploaded.
                  </p>
                )}
              </>
            )}
          </>
        )}
      </LoadingSpinner>
    </>
  );
};

export default ReleaseMetaGuidanceSection;
