import DetailsMenu from '@common/components/DetailsMenu';
import { FormFieldCheckboxGroup } from '@common/components/form';
import FormFieldCheckboxSearchGroup, {
  FormFieldCheckboxSearchGroupProps,
} from '@common/components/form/FormFieldCheckboxSearchGroup';
import FormCheckboxSelectionCount from '@frontend/modules/table-tool/components/FormCheckboxSelectedCount';
import React, { useEffect, useState } from 'react';

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
          {...props}
          hideCount
          legendHidden
          selectAll
          name={name}
          options={options}
        />
      ) : (
        <FormFieldCheckboxGroup
          {...props}
          selectAll
          name={name}
          options={options}
        />
      )}
    </DetailsMenu>
  );
};

export default FormFieldCheckboxMenu;
