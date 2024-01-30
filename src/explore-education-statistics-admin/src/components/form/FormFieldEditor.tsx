import { Element, Node } from '@admin/types/ckeditor';
import FormEditor, {
  EditorElementsHandler,
  FormEditorProps,
} from '@admin/components/form/FormEditor';
import { useFormIdContext } from '@common/components/form/contexts/FormIdContext';
import FormGroup from '@common/components/form/FormGroup';
import { OmitStrict } from '@common/types';
import createErrorHelper from '@common/validation/createErrorHelper';
import WarningMessage from '@common/components/WarningMessage';
import useToggle from '@common/hooks/useToggle';
import Yup from '@common/validation/yup';
import { Field, FieldProps } from 'formik';
import React, { useRef, useState } from 'react';

interface InvalidUrl {
  text: string;
  url: string;
}

export const elementsFieldName = (name: string) => `__${name}`;

type Props<FormValues> = {
  formGroupClass?: string;
  id?: string;
  name: keyof FormValues | string;
  shouldValidateAltText?: boolean;
  shouldValidateLinks?: boolean;
  showError?: boolean;
  testId?: string;
  onBlur?: (isDirty: boolean) => void;
} & OmitStrict<FormEditorProps, 'id' | 'value' | 'onChange' | 'onBlur'>;

function FormFieldEditor<T>({
  error,
  formGroupClass,
  id,
  name,
  shouldValidateAltText = true,
  shouldValidateLinks = true,
  showError = true,
  testId,
  onBlur,
  ...props
}: Props<T>) {
  const { fieldId } = useFormIdContext();
  const elements = useRef<Element[]>([]);

  const [altTextError, toggleAltTextError] = useToggle(false);
  const [invalidLinkErrors, setInvalidLinkErrors] = useState<InvalidUrl[]>([]);

  function validateAltText(els: Element[]): string {
    const hasInvalidImage = els.some(
      element =>
        isInvalidImage(element) ||
        Array.from(element.getChildren()).some(child => isInvalidImage(child)),
    );
    toggleAltTextError(hasInvalidImage);
    return hasInvalidImage ? 'All images must have alternative text. ' : '';
  }

  function validateLinks(els: Element[]): string {
    const invalidLinks = getInvalidLinks(els);
    setInvalidLinkErrors(invalidLinks);
    return invalidLinks.length
      ? `${
          invalidLinks.length === 1
            ? '1 link has an invalid URL.'
            : `${invalidLinks.length} links have invalid URLs.`
        }`
      : '';
  }

  return (
    <Field
      name={name}
      validate={() => {
        const invalidLinksError = shouldValidateLinks
          ? validateLinks(elements.current)
          : '';
        const invalidAltTextError = shouldValidateAltText
          ? validateAltText(elements.current)
          : '';
        return `${invalidAltTextError}${invalidLinksError}`;
      }}
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

function isInvalidImage(element: Element | Node) {
  return (
    (element.name === 'imageBlock' || element.name === 'imageInline') &&
    !element.getAttribute('alt')
  );
}

function getInvalidLinks(elements: Element[]) {
  return elements
    .flatMap(element =>
      Array.from(element.getChildren()).flatMap(child => child),
    )
    .reduce<InvalidUrl[]>((acc, el) => {
      if (!el.getAttribute('linkHref')) {
        return acc;
      }
      const jsonEl = el.toJSON();
      const attributes = jsonEl.attributes as Record<string, unknown>;
      const url = attributes.linkHref as string;

      try {
        // exclude anchor links and localhost as they fail Yup url validation.
        if (
          url &&
          !url.startsWith('#') &&
          !url.startsWith('http://localhost')
        ) {
          Yup.string().url().validateSync(url.trim());
        }
      } catch {
        acc.push({
          text: jsonEl.data as string,
          url,
        });
      }
      return acc;
    }, []);
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
