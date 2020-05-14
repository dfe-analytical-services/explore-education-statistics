import DetailsMenu from '@common/components/DetailsMenu';
import { FormFieldCheckboxGroup } from '@common/components/form';
import FormFieldCheckboxSearchGroup from '@common/components/form/FormFieldCheckboxSearchGroup';
import FormFieldCheckboxSearchSubGroups, {
  FormFieldCheckboxSearchSubGroupsProps,
} from '@common/components/form/FormFieldCheckboxSearchSubGroups';
import { useField } from 'formik';
import React, { useEffect, useState } from 'react';
import FormCheckboxSelectionCount from './FormCheckboxSelectedCount';

const FormFieldCheckboxGroupsMenu = <T extends {}>(
  props: FormFieldCheckboxSearchSubGroupsProps<T>,
) => {
  const { name, options, onAllChange, legend } = props;
  const [open, setOpen] = useState(false);

  const [, meta] = useField(name);

  useEffect(() => {
    if (meta.error && meta.touched) {
      setOpen(true);
    }
  }, [meta.error, meta.touched]);

  const renderSingleGroup = () => {
    return options[0].options.length > 1 ? (
      <FormFieldCheckboxSearchGroup
        {...props}
        hideCount
        legendHidden
        selectAll
        options={options[0].options}
        onAllChange={(event, checked) => {
          if (onAllChange) {
            onAllChange(event, checked, options[0].options);
          }
        }}
      />
    ) : (
      <FormFieldCheckboxGroup
        {...props}
        selectAll
        small
        name={name}
        options={options[0].options}
        onAllChange={(event, checked) => {
          if (onAllChange) {
            onAllChange(event, checked, options[0].options);
          }
        }}
      />
    );
  };

  return (
    <DetailsMenu
      open={open}
      jsRequired
      onToggle={(isOpen, event) => {
        if (meta.error && meta.touched) {
          event.preventDefault();
        }
      }}
      summary={
        <>
          {legend}
          <FormCheckboxSelectionCount name={name} />
        </>
      }
    >
      {options.length > 1 && (
        <FormFieldCheckboxSearchSubGroups {...props} hideCount legendHidden />
      )}

      {options.length === 1 && renderSingleGroup()}
    </DetailsMenu>
  );
};

export default FormFieldCheckboxGroupsMenu;
