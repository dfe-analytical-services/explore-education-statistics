import fieldId from '@common/components/form/util/fieldId';
import React, { createContext, ReactNode, useContext, useMemo } from 'react';

interface FormIdContextValue {
  formId: string;
  /**
   * Create an `id` string for a field based on its
   * {@param name} and the form context's {@member formId}.
   * If a {@param customId} is provided, then that will be
   * used instead of the name.
   */
  fieldId: (name: string, customId?: string) => string;
  /**
   * Prefix an {@param id} with the
   * context's {@member formId}. Useful for
   * ids that don't use a field name.
   */
  prefixFormId: (id: string) => string;
}

const FormIdContext = createContext<FormIdContextValue>({
  formId: '',
  fieldId: (name, id) => id ?? fieldId('', name),
  prefixFormId: id => id,
});

interface FormIdContextProviderProps {
  children: ReactNode;
  id: string;
}

export const FormIdContextProvider = ({
  children,
  id: formId,
}: FormIdContextProviderProps) => {
  const value = useMemo<FormIdContextValue>(() => {
    const prefixFormId: FormIdContextValue['prefixFormId'] = customId => {
      return `${formId}-${customId}`;
    };

    return {
      formId,
      fieldId: (name, customId) =>
        customId ? prefixFormId(customId) : fieldId(formId, name),
      prefixFormId,
    };
  }, [formId]);

  return (
    <FormIdContext.Provider value={value}>{children}</FormIdContext.Provider>
  );
};

export function useFormIdContext(): FormIdContextValue {
  return useContext(FormIdContext);
}
