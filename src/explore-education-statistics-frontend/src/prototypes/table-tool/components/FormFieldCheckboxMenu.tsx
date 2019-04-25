import DetailsMenu from '@common/components/DetailsMenu';
import { FormCheckboxSearchGroupProps } from '@common/components/form/FormCheckboxSearchGroup';
import FormFieldCheckboxSearchGroup from '@common/components/form/FormFieldCheckboxSearchGroup';
import { Omit } from '@common/types/util';
import FormCheckboxSelectionCount from '@frontend/prototypes/table-tool/components/FormCheckboxSelectedCount';
import React from 'react';

export type FormFieldFilterMenuProps<FormValues> = {
  name: keyof FormValues | string;
  summary: string;
} & Omit<FormCheckboxSearchGroupProps, 'onChange' | 'onAllChange' | 'value'>;

const FormFieldCheckboxMenu = <T extends {}>(
  props: FormFieldFilterMenuProps<T>,
) => {
  const { name, options, summary } = props;

  return (
    <DetailsMenu
      summary={
        <>
          {summary}
          <FormCheckboxSelectionCount name={name} />
        </>
      }
    >
      <FormFieldCheckboxSearchGroup
        {...props}
        hideCount
        name={name}
        options={options}
      />
    </DetailsMenu>
  );
};

export default FormFieldCheckboxMenu;
