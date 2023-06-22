import { Element } from '@admin/types/ckeditor';
import FormEditor, {
  EditorElementsHandler,
  FormEditorProps,
} from '@admin/components/form/FormEditor';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormGroup from '@common/components/form/FormGroup';
import { OmitStrict } from '@common/types';
import createErrorHelper from '@common/validation/createErrorHelper';
import { Field, FieldProps } from 'formik';
import React, { useRef } from 'react';

export const elementsFieldName = (name: string) => `__${name}`;

type Props<FormValues> = {
  formGroupClass?: string;
  id?: string;
  name: keyof FormValues | string;
  showError?: boolean;
  testId?: string;
  validateElements?: (
    elements: Element[],
  ) => string | undefined | Promise<string | undefined>;
  onBlur?: (isDirty: boolean) => void;
} & OmitStrict<FormEditorProps, 'id' | 'value' | 'onChange' | 'onBlur'>;

function FormFieldEditor<T>({
  error,
  formGroupClass,
  id,
  name,
  showError = true,
  testId,
  validateElements,
  onBlur,
  ...props
}: Props<T>) {
  const { fieldId } = useFormIdContext();
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
              id={fieldId(name as string, id)}
              onBlur={() => {
                form.setFieldTouched(name as string, true);
                if (onBlur) {
                  onBlur(form.dirty);
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
