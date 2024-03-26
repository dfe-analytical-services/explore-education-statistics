import { Element } from '@admin/types/ckeditor';
import FormEditor, {
  EditorElementsHandler,
  FormEditorProps,
} from '@admin/components/form/FormEditor';
import { InvalidUrl } from '@admin/components/editable/EditableContentForm';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormGroup from '@common/components/form/FormGroup';
import { OmitStrict } from '@common/types';
import WarningMessage from '@common/components/WarningMessage';
import useRegister from '@common/components/form/rhf/hooks/useRegister';
import getErrorMessage from '@common/components/form/rhf/util/getErrorMessage';
import React, { useRef } from 'react';
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
  altTextError?: string;
  name: Path<TFormValues>;
  formGroupClass?: string;
  id?: string;
  invalidLinkErrors?: InvalidUrl[];
  shouldValidateLinks?: boolean;
  showError?: boolean;
  testId?: string;
  onBlur?: (isDirty: boolean) => void;
  onChange?: (elements?: Element[]) => void;
}

export default function RHFFormFieldEditor<TFormValues extends FieldValues>({
  altTextError,
  error,
  formGroupClass,
  id,
  invalidLinkErrors = [],
  name,
  showError = true,
  testId,
  onBlur,
  onChange,
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
      {altTextError && <AltTextWarningMessage />}

      {invalidLinkErrors.length > 0 && (
        <WarningMessage className="govuk-!-margin-bottom-1">
          The following links have invalid URLs:
          <ul className="govuk-!-font-weight-regular govuk-!-margin-bottom-1 govuk-!-margin-top-1">
            {invalidLinkErrors.map(link => (
              <li key={link?.text}>
                {link?.text} ({link?.url})
              </li>
            ))}
          </ul>
        </WarningMessage>
      )}

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
        onElementsReady={handleElements}
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
    </FormGroup>
  );
}

export function AltTextWarningMessage() {
  return (
    <WarningMessage>
      Alternative text must be added for images, for guidance see{' '}
      <a
        href="https://www.w3.org/WAI/tutorials/images/tips/"
        rel="noopener noreferrer"
        target="_blank"
      >
        W3C tips on writing alternative text
      </a>
      . <br />
      Images without alternative text are outlined in red.
    </WarningMessage>
  );
}
