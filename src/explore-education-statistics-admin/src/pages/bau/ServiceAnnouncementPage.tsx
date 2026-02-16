import Page from '@admin/components/Page';
import React, { useCallback, useMemo, useState } from 'react';
import FormProvider from '@common/components/form/FormProvider';
import FormGroup from '@common/components/form/FormGroup';
import FormFieldset from '@common/components/form/FormFieldset';
import Button from '@common/components/Button';
import ButtonGroup from '@common/components/ButtonGroup';
import Form from '@common/components/form/Form';
import FormFieldTextInput from '@common/components/form/FormFieldTextInput';
import Yup from '@common/validation/yup';
import serviceAnnouncementService from '@admin/services/serviceAnnouncementService';
import { ObjectSchema } from 'yup';
import { useNotificationHubContext } from '@admin/contexts/NotificationHubContext';
import ModalConfirm from '@common/components/ModalConfirm';
import useToggle from '@common/hooks/useToggle';
import InsetText from '@common/components/InsetText';

const ServiceAnnouncementPage = () => {
  interface FormValues {
    message: string;
  }

  const { hub } = useNotificationHubContext();
  const [showConfirmModal, toggleConfirmModal] = useToggle(false);
  const [messagePreview, setMessagePreview] = useState<string>('');

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      message: Yup.string().required('Enter a message to send'),
    });
  }, []);

  const handleSubmit = useCallback(
    async (values: FormValues) => {
      await serviceAnnouncementService.broadcastMessage({
        message: values.message,
        connectionId: hub.connectionId ?? '',
      });
    },
    [hub],
  );

  return (
    <Page
      title="Service announcement"
      caption="Instantly broadcast a message to all connected users"
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Service announcement' },
      ]}
    >
      <FormProvider enableReinitialize validationSchema={validationSchema}>
        {form => {
          return (
            <>
              <Form
                id="broadcastMessageForm"
                onSubmit={() => {
                  setMessagePreview(form.getValues('message'));
                  toggleConfirmModal.on();
                }}
              >
                <FormFieldset
                  id="broadcast-message"
                  legend="Broadcast message"
                  legendSize="l"
                  hint="This message will be instantly broadcast to all connected users."
                >
                  <FormGroup>
                    <FormFieldTextInput<FormValues>
                      id="message"
                      name="message"
                      label="Message text"
                      className="govuk-!-width-one-half"
                    />
                  </FormGroup>

                  <ButtonGroup>
                    <Button type="submit">Broadcast message</Button>
                  </ButtonGroup>
                </FormFieldset>
              </Form>

              {showConfirmModal && (
                <ModalConfirm
                  title="Confirm broadcast message"
                  onConfirm={async () => {
                    await form.handleSubmit(async values => {
                      await handleSubmit(values);
                      form.reset();
                      setMessagePreview('');
                      toggleConfirmModal.off();
                    })();
                  }}
                  onExit={toggleConfirmModal.off}
                  onCancel={toggleConfirmModal.off}
                  open
                >
                  <p>
                    The following message will be sent to all connected users:
                  </p>
                  <InsetText>{messagePreview}</InsetText>
                  <p>Are you sure you want to send this message?</p>
                </ModalConfirm>
              )}
            </>
          );
        }}
      </FormProvider>
    </Page>
  );
};

export default ServiceAnnouncementPage;
