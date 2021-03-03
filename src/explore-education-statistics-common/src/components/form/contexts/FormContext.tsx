import fieldId from '@common/components/form/util/fieldId';
import React, { createContext, ReactNode, useContext, useMemo } from 'react';

interface FormContextValue {
  formId: string;
  /**
   * Create an `id` string for a field
   * based on its {@param name} and the
   * form context's {@member formId}.
   */
  fieldId: (name: string) => string;
  /**
   * Prefix an {@param id} with the
   * context's {@member formId}. Useful for
   * ids that don't use a field name.
   */
  prefixFormId: (id: string) => string;
}

const FormContext = createContext<FormContextValue>({
  formId: '',
  fieldId: name => fieldId('', name),
  prefixFormId: id => id,
});

interface FormContextProviderProps {
  children: ReactNode;
  id: string;
}

export const FormContextProvider = ({
  children,
  id: formId,
}: FormContextProviderProps) => {
  const value = useMemo<FormContextValue>(() => {
    return {
      formId,
      fieldId: name => fieldId(formId, name),
      prefixFormId: customId => `${formId}-${customId}`,
    };
  }, [formId]);

  return <FormContext.Provider value={value}>{children}</FormContext.Provider>;
};

export function useFormContext(): FormContextValue {
  return useContext(FormContext);
}
