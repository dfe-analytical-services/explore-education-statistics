import FormFieldRadioSearchGroup from '@common/components/form/FormFieldRadioSearchGroup';
import { mapFieldErrors } from '@common/validation/serverValidations';
import Button from '@common/components/Button';
import Details from '@common/components/Details';
import FormattedDate from '@common/components/FormattedDate';
import SummaryList from '@common/components/SummaryList';
import SummaryListItem from '@common/components/SummaryListItem';
import ButtonGroup from '@common/components/ButtonGroup';
import ButtonText from '@common/components/ButtonText';
import getMethodologyApprovalStatusLabel from '@admin/pages/methodology/utils/getMethodologyApprovalStatusLabel';
import { MethodologyVersion } from '@admin/services/methodologyService';
import Tag from '@common/components/Tag';
import TagGroup from '@common/components/TagGroup';
import { RadioOption } from '@common/components/form/FormRadioGroup';
import FormProvider from '@common/components/form/FormProvider';
import Form from '@common/components/form/Form';
import Yup from '@common/validation/yup';
import React, { useMemo } from 'react';
import { ObjectSchema } from 'yup';
import VisuallyHidden from '@common/components/VisuallyHidden';

const errorMappings = [
  mapFieldErrors<FormValues>({
    target: 'methodologyId',
    messages: {
      CannotAdoptMethodologyAlreadyLinkedToPublication:
        'Select a methodology that has not already been adopted by this publication',
    },
  }),
];

interface FormValues {
  methodologyId: string;
}

interface Props {
  methodologies: MethodologyVersion[];
  onCancel: () => void;
  onSubmit: (values: FormValues) => void;
}

const AdoptMethodologyForm = ({ methodologies, onCancel, onSubmit }: Props) => {
  const radioOptions = useMemo<RadioOption[]>(
    () =>
      methodologies.map(methodology => {
        return {
          label: methodology.title,
          value: methodology.methodologyId,
          hint: (
            <Details
              summary="More details"
              summaryAfter={
                <VisuallyHidden>{` about ${methodology.owningPublication.title}`}</VisuallyHidden>
              }
              className="govuk-!-margin-bottom-2"
            >
              <SummaryList className="govuk-!-margin-bottom-3">
                <SummaryListItem term="Owning publication">
                  {methodology.owningPublication?.title}
                </SummaryListItem>
                <SummaryListItem term="Status">
                  <TagGroup>
                    <Tag>
                      {getMethodologyApprovalStatusLabel(methodology.status)}
                    </Tag>
                    {methodology.amendment && <Tag>Amendment</Tag>}
                  </TagGroup>
                </SummaryListItem>
                <SummaryListItem term="Publish date">
                  {methodology.published ? (
                    <FormattedDate>{methodology.published}</FormattedDate>
                  ) : (
                    'Not yet published'
                  )}
                </SummaryListItem>
              </SummaryList>
            </Details>
          ),
        };
      }),
    [methodologies],
  );

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      methodologyId: Yup.string().required(
        'Select a published methodology to adopt',
      ),
    });
  }, []);

  return (
    <FormProvider
      enableReinitialize
      errorMappings={errorMappings}
      initialValues={{ methodologyId: '' }}
      validationSchema={validationSchema}
    >
      <Form id="adoptMethodologyForm" onSubmit={onSubmit}>
        <FormFieldRadioSearchGroup
          id="selectMethodology"
          legend="Select a published methodology"
          name="methodologyId"
          options={radioOptions}
          searchLabel="Search for a published methodology"
        />
        <ButtonGroup>
          <Button type="submit">Save</Button>
          <ButtonText onClick={onCancel}>Cancel</ButtonText>
        </ButtonGroup>
      </Form>
    </FormProvider>
  );
};

export default AdoptMethodologyForm;
