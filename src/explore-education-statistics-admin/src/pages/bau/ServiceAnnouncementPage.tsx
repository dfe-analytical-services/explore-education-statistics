import Page from '@admin/components/Page';
import React, { useCallback, useMemo } from 'react';
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

const ServiceAnnouncementPage = () => {
  interface FormValues {
    message: string;
  }

  const { hub } = useNotificationHubContext();

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      message: Yup.string().required('Enter a message to send'),
    });
  }, []);

  const handleSubmit = useCallback(
    async (values: FormValues) => {
      await serviceAnnouncementService.broadcastMessage({
        message: values.message,
        // @ts-expect-error: Property 'connection' is protected and only accessible within class 'Hub' and its subclasses.
        connectionId: hub.connection.connectionId,
      });
    },
    [hub],
  );

  return (
    <Page
      title="Service announcement"
      caption="Instantly broascast a message to all connected users"
      wide
      breadcrumbs={[
        { name: 'Platform administration', link: '/administration' },
        { name: 'Service announcement' },
      ]}
    >
      <FormProvider
        enableReinitialize
        validationSchema={validationSchema}
        resetAfterSubmit
      >
        <Form id="broadcastMessageForm" onSubmit={handleSubmit}>
          <FormFieldset
            id="message"
            legend="Broadcast message"
            legendSize="l"
            hint="This message will be instantly broadcast to all connected users."
          >
            <FormGroup>
              <FormFieldTextInput<FormValues>
                id="label"
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
      </FormProvider>
    </Page>
  );
};

export default ServiceAnnouncementPage;
