import DetailsMenu from '@common/components/DetailsMenu';
import {
  FormFieldCheckboxGroup,
  FormFieldCheckboxSearchGroup,
} from '@common/components/form';
import { FormFieldCheckboxSearchGroupProps } from '@common/components/form/FormFieldCheckboxSearchGroup';
import React, { useEffect, useState } from 'react';
import FormCheckboxSelectionCount from './FormCheckboxSelectedCount';

const FormFieldCheckboxMenu = <T extends {}>(
  props: FormFieldCheckboxSearchGroupProps<T>,
) => {
  const { error, name, options, legend } = props;
  const [open, setOpen] = useState();

  useEffect(() => {
    if (error) {
      setOpen(true);
    }
  }, [error]);

  return (
    <DetailsMenu
      open={open}
      summary={
        <>
          {legend}
          <FormCheckboxSelectionCount name={name} />
        </>
      }
    >
      {options.length > 1 ? (
        <FormFieldCheckboxSearchGroup
          selectAll
          hideCount
          legendHidden
          {...props}
          name={name}
          options={options}
        />
      ) : (
        <FormFieldCheckboxGroup
          selectAll
          {...props}
          name={name}
          options={options}
        />
      )}
    </DetailsMenu>
  );
};

export default FormFieldCheckboxMenu;
