import PrototypeAPIDataSetPreview from '@admin/prototypes/admin-api/components/PrototypeAPIDataSetPreview';
import {
  PrototypeNotification,
  PrototypeSubject,
} from '@admin/prototypes/admin-api/PrototypePublicationSubjects';
import ButtonText from '@common/components/ButtonText';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Yup from '@common/validation/yup';
import React, { useState } from 'react';
import FormProvider from '@common/components/form/rhf/FormProvider';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldCheckboxGroup from '@common/components/form/rhf/RHFFormFieldCheckboxGroup';
import RHFFormFieldTextArea from '@common/components/form/rhf/RHFFormFieldTextArea';

interface FormValues {
  summary: string;
  channels: string[];
}

interface Props {
  publicationSubject?: PrototypeSubject;
  onClose: () => void;
  onSubmit: (notification: PrototypeNotification) => void;
}

const PrototypeNotificationCreate = ({
  publicationSubject,
  onClose,
  onSubmit,
}: Props) => {
  const [preview, setPreview] = useState<string>('');

  const [notification, setNotification] = useState<FormValues>();

  if (preview) {
    return (
      <PrototypeAPIDataSetPreview
        notificationSummary={preview}
        publicationSubject={publicationSubject}
        onClose={() => setPreview('')}
        onSubmit={() => {
          if (notification && publicationSubject) {
            onSubmit({ ...notification, subjectId: publicationSubject?.id });
          }
        }}
      />
    );
  }

  return (
    <>
      <ButtonText
        className="govuk-!-margin-bottom-6 govuk-!-padding-left-3 govuk-link govuk-back-link"
        onClick={onClose}
      >
        Back to API data sets
      </ButtonText>

      <section className="govuk-!-width-three-quarters">
        <span className="govuk-caption-l">{publicationSubject?.title}</span>

        <h2>Publish notification of upcoming API changes</h2>
        <p>
          If you wish to publish a notification of upcoming changes describing
          the changes in this data set please complete the form below. Describe
          any expected breaking changes of updates in the next data set version.
        </p>
        <FormProvider
          initialValues={{
            channels: notification?.channels || ['banner', 'email'],
            summary: notification?.summary || '',
          }}
          validationSchema={Yup.object({
            subjectId: Yup.string().required('Enter a summary'),
            channels: Yup.array().required(),
            summary: Yup.string().required(),
          })}
        >
          {({ watch }) => {
            const values = watch();
            return (
              <RHFForm id="form" onSubmit={() => {}}>
                <RHFFormFieldTextArea
                  label={
                    <span className="govuk-!-font-weight-bold">
                      Summary of updates
                    </span>
                  }
                  name="summary"
                  hint=" Describe any expected breaking changes of updates in the next data set version."
                />

                <RHFFormFieldCheckboxGroup
                  legend="Notification channels"
                  legendSize="s"
                  name="channels"
                  options={[
                    {
                      label: 'Banner in API guidance',
                      value: 'banner',
                    },
                    {
                      label: 'Email subscriber lists',
                      value: 'email',
                    },
                  ]}
                />
                <ButtonGroup>
                  <Button
                    type="button"
                    onClick={() => {
                      setNotification(values);
                      setPreview(values.summary);
                    }}
                  >
                    Preview and send notification
                  </Button>
                  <Button onClick={onClose} variant="secondary">
                    Cancel
                  </Button>
                </ButtonGroup>
              </RHFForm>
            );
          }}
        </FormProvider>
      </section>
    </>
  );
};

export default PrototypeNotificationCreate;
