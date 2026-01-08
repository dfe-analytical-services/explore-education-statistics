import { Element } from '@admin/types/ckeditor';
import FormEditor, {
  EditorElementsHandler,
  FormEditorProps,
} from '@admin/components/form/FormEditor';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormGroup from '@common/components/form/FormGroup';
import { OmitStrict } from '@common/types';
import useRegister from '@common/components/form/hooks/useRegister';
import getErrorMessage from '@common/components/form/util/getErrorMessage';
import Details from '@common/components/Details';
import React, { ReactNode, useRef } from 'react';
import {
  FieldValues,
  Path,
  PathValue,
  useFormContext,
  useWatch,
} from 'react-hook-form';

export const elementsFieldName = (name: string) => `__${name}`;

export interface Props<TFormValues extends FieldValues>
  extends OmitStrict<FormEditorProps, 'id' | 'value' | 'onChange' | 'onBlur'> {
  contentErrorDetails?: ReactNode;
  name: Path<TFormValues>;
  formGroupClass?: string;
  id?: string;
  shouldValidateLinks?: boolean;
  showError?: boolean;
  testId?: string;
  onBlur?: (isDirty: boolean) => void;
  onChange?: (elements?: Element[]) => void;
}

export default function FormFieldEditor<TFormValues extends FieldValues>({
  contentErrorDetails,
  error,
  formGroupClass,
  id,
  name,
  showError = true,
  testId,
  onBlur,
  onChange,
  onElementsReady,
  ...props
}: Props<TFormValues>) {
  const {
    formState: { errors },
    register,
    getValues,
    setValue,
    getFieldState,
  } = useFormContext<TFormValues>();
  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const { ref, ...field } = useRegister(name, register);
  const value = useWatch({ name }) || '';
  const { fieldId } = useFormIdContext();
  const elements = useRef<Element[]>([]);

  const handleElements: EditorElementsHandler = nextElements => {
    elements.current = nextElements;
  };
  const errorMessage = error || getErrorMessage(errors, name, showError);

  return (
    <FormGroup hasError={!!errorMessage} className={formGroupClass}>
      {contentErrorDetails}

      <FormEditor
        testId={testId}
        {...props}
        {...field}
        id={fieldId(name as string, id)}
        value={value}
        onBlur={() => {
          const currentValue = getValues(name);
          setValue(name, currentValue, {
            shouldDirty: true,
            shouldTouch: true,
            shouldValidate: true,
          });

          if (onBlur) {
            const fieldState = getFieldState(name);
            onBlur(fieldState.isDirty);
          }
        }}
        onElementsChange={handleElements}
        onElementsReady={els => {
          handleElements(els);
          onElementsReady?.(els);
        }}
        onChange={nextValue => {
          setValue(
            name,
            nextValue as PathValue<TFormValues, Path<TFormValues>>,
            { shouldDirty: true, shouldTouch: true, shouldValidate: true },
          );

          onChange?.(elements.current);
        }}
        error={errorMessage}
      />
      <Details summary="Help using the editor with a keyboard">
        <p>
          To access the toolbar press Alt + F10 (⌥ F10 on Mac), use the arrow
          keys to navigate between toolbar buttons and Space or Enter to select
          an option.
        </p>
        <p>
          For a full list of available keyboard shortcuts press Alt + 0 (⌥ 0 on
          Mac).
        </p>
      </Details>
    </FormGroup>
  );
}
