import DetailsMenu from '@common/components/DetailsMenu';
import RHFFormFieldCheckboxSearchSubGroups, {
  RHFFormFieldCheckboxSearchSubGroupsProps,
} from '@common/components/form/rhf/RHFFormFieldCheckboxSearchSubGroups';
import RHFFormCheckboxSelectedCount from '@common/components/form/rhf/RHFFormCheckboxSelectedCount';
import { OmitStrict } from '@common/types';
import React from 'react';
import { FieldValues, useFormContext } from 'react-hook-form';
import get from 'lodash/get';

interface Props<TFormValues extends FieldValues>
  extends OmitStrict<
    RHFFormFieldCheckboxSearchSubGroupsProps<TFormValues>,
    'legendHidden'
  > {
  hiddenText?: string;
  legend: string;
  open?: boolean;
}

export default function RHFFormFieldCheckboxGroupsMenu<
  TFormValues extends FieldValues
>(props: Props<TFormValues>) {
  const { hiddenText, legend, name, open = false } = props;

  const {
    formState: { errors },
  } = useFormContext();

  return (
    <DetailsMenu
      open={open}
      hiddenText={hiddenText}
      jsRequired
      preventToggle={!!get(errors, name)}
      summary={legend}
      summaryAfter={<RHFFormCheckboxSelectedCount name={name} />}
    >
      <RHFFormFieldCheckboxSearchSubGroups {...props} legendHidden />
    </DetailsMenu>
  );
}
