import ButtonText from '@common/components/ButtonText';
import { Form, FormFieldTextInput } from '@common/components/form';
import Button from '@common/components/Button';
import Yup from '@common/validation/yup';
import React from 'react';
import { Formik } from 'formik';
import { PublicationSubject } from '../PrototypePublicationSubjects';

interface FormValues {
  title: string;
}

interface Props {
  publicationSubject: PublicationSubject;
  onClose: () => void;
  onSubmit: (updatedPublicationSubject: PublicationSubject) => void;
}

const PrototypeEditPublicationSubjectTitle = ({
  publicationSubject,
  onClose,
  onSubmit,
}: Props) => {
  return (
    <>
      <ButtonText
        className="govuk-!-margin-bottom-6 govuk-!-padding-left-3 govuk-link govuk-back-link"
        onClick={onClose}
      >
        Back to API data sets
      </ButtonText>

      <section>
        <span className="govuk-caption-l">{publicationSubject.title}</span>
        <h2>Edit data set title</h2>

        <Formik<FormValues>
          initialValues={{ title: publicationSubject.title }}
          validationSchema={Yup.object<FormValues>({
            title: Yup.string().required('Enter a title'),
          })}
          onSubmit={values => {
            onSubmit({
              ...publicationSubject,
              title: values.title,
            });
          }}
        >
          <Form id="dataFileForm">
            <FormFieldTextInput<FormValues>
              className="govuk-!-width-two-thirds"
              label="Title"
              name="title"
            />

            <Button type="submit">Save</Button>
          </Form>
        </Formik>
      </section>
    </>
  );
};

export default PrototypeEditPublicationSubjectTitle;
