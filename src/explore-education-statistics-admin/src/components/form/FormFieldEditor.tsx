import { Element } from '@admin/types/ckeditor';
import FormEditor, {
  EditorElementsHandler,
  FormEditorProps,
} from '@admin/components/form/FormEditor';
import { useFormContext } from '@common/components/form/contexts/FormContext';
import FormGroup from '@common/components/form/FormGroup';
import { OmitStrict } from '@common/types';
import createErrorHelper from '@common/validation/createErrorHelper';
import { Field, FieldProps } from 'formik';
import React, { useRef } from 'react';

export const elementsFieldName = (name: string) => `__${name}`;

type Props<FormValues> = {
  id?: string;
  name: keyof FormValues | string;
  formGroupClass?: string;
  handleBlur?: (isDirty: boolean) => void;
  showError?: boolean;
  testId?: string;
  validateElements?: (
    elements: Element[],
  ) => string | undefined | Promise<string | undefined>;
} & OmitStrict<FormEditorProps, 'id' | 'value' | 'onChange'>;

function FormFieldEditor<T>({
  error,
  id,
  name,
  showError = true,
  formGroupClass,
  testId,
  validateElements,
  handleBlur,
  ...props
}: Props<T>) {
  const { prefixFormId, fieldId } = useFormContext();
  const elements = useRef<Element[]>([]);

  return (
    <Field
      name={name}
      validate={() =>
        validateElements ? validateElements(elements.current) : undefined
      }
    >
      {({ field, form }: FieldProps) => {
        const { getError } = createErrorHelper(form);

        let errorMessage = error || getError(name);

        if (!showError) {
          errorMessage = '';
        }

        const handleElements: EditorElementsHandler = nextElements => {
          elements.current = nextElements;
        };

        return (
          <FormGroup hasError={!!errorMessage} className={formGroupClass}>
            <FormEditor
              testId={testId}
              {...props}
              {...field}
              id={id ? prefixFormId(id) : fieldId(name as string)}
              onBlur={() => {
                form.setFieldTouched(name as string, true);
                if (handleBlur) {
                  handleBlur(form.dirty);
                }
              }}
              onElementsChange={handleElements}
              onElementsReady={handleElements}
              onChange={value => {
                form.setFieldValue(name as string, value);
              }}
              error={errorMessage}
            />
          </FormGroup>
        );
      }}
    </Field>
  );
}

export default FormFieldEditor;
