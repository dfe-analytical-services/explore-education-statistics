import { useConfig } from '@admin/contexts/ConfigContext';
import glossaryQueries from '@admin/queries/glossaryQueries';
import { GlossaryItem } from '@admin/types/ckeditor';
import Button from '@common/components/Button';
import FormComboBox from '@common/components/form/FormComboBox';
import RHFForm from '@common/components/form/rhf/RHFForm';
import RHFFormFieldTextInput from '@common/components/form/rhf/RHFFormFieldTextInput';
import createRHFErrorHelper from '@common/components/form/rhf/validation/createRHFErrorHelper';
import FormProvider from '@common/components/form/rhf/FormProvider';
import ButtonGroup from '@common/components/ButtonGroup';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { GlossaryEntry } from '@common/services/types/glossary';
import Yup from '@common/validation/yup';
import LoadingSpinner from '@common/components/LoadingSpinner';
import React, { useMemo, useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { ObjectSchema } from 'yup';

interface FormValues {
  slug: string;
  text: string;
}

interface Props {
  onCancel: () => void;
  onSubmit: (item: GlossaryItem) => void;
}

export default function GlossaryItemInsertForm({ onCancel, onSubmit }: Props) {
  const config = useConfig();

  const { data: glossary = [], isLoading } = useQuery(glossaryQueries.list);

  const allGlossaryEntries = useMemo(
    () => glossary.flatMap(g => g.entries),
    [glossary],
  );

  const [searchResults, setSearchResults] = useState<GlossaryEntry[]>([]);
  const [selectedEntry, setSelectedEntry] = useState<GlossaryEntry>();

  const [search] = useDebouncedCallback((value: string) => {
    if (value.length < 3) {
      setSearchResults([]);
      return;
    }
    const nextSearchResults = value.length
      ? allGlossaryEntries.filter(element => {
          return element.title.toLowerCase().includes(value.toLowerCase());
        })
      : [];
    setSearchResults(nextSearchResults);
  }, 400);

  const validationSchema = useMemo<ObjectSchema<FormValues>>(() => {
    return Yup.object({
      slug: Yup.string().required('Select a glossary entry'),
      text: Yup.string().required('Enter link text'),
    });
  }, []);

  const handleFormSubmit = (values: FormValues) =>
    onSubmit({
      text: values.text,
      url: `${config.publicAppUrl}/glossary#${values.slug}`,
    });

  if (isLoading) {
    return <LoadingSpinner hideText text="Loading glossary" />;
  }

  return (
    <FormProvider validationSchema={validationSchema}>
      {({ formState, getValues, handleSubmit, setValue, trigger }) => {
        const { getError } = createRHFErrorHelper({
          errors: formState.errors,
          touchedFields: formState.touchedFields,
          isSubmitted: true,
        });

        const slug = getValues('slug');

        return (
          <RHFForm
            id="featuredTablesForm"
            showErrorSummary={false}
            onSubmit={handleFormSubmit}
          >
            <FormComboBox
              error={getError('slug')}
              hideSearchIcon={!!selectedEntry}
              id="glossarySearch"
              inputLabel="Glossary entry"
              inputValue={selectedEntry?.title}
              options={searchResults.map(result => result.title)}
              overflowContainer={false}
              onInputChange={event => {
                search(event.target.value);
              }}
              onSelect={selectedIndex => {
                setValue('text' as const, searchResults[selectedIndex].title, {
                  shouldTouch: true,
                });
                setValue('slug' as const, searchResults[selectedIndex].slug, {
                  shouldTouch: true,
                });
                trigger('slug');
                setSelectedEntry(searchResults[selectedIndex]);
              }}
            />

            {slug && (
              <>
                <p className="govuk-!-margin-top-1">Slug: {slug}</p>

                <RHFFormFieldTextInput<FormValues>
                  formGroupClass="govuk-!-margin-top-5 govuk-!-margin-bottom-2"
                  id="text"
                  label="Link text"
                  name="text"
                />
              </>
            )}

            <ButtonGroup className="govuk-!-margin-top-5">
              <Button
                type="submit"
                onClick={event => {
                  // Have to prevent default here as the form is nested,
                  // despite being in a modal, so otherwise it submits
                  // the parent EditableContentForm.
                  event.preventDefault();
                  handleSubmit(handleFormSubmit)();
                }}
              >
                Insert
              </Button>
              <Button variant="secondary" onClick={onCancel}>
                Cancel
              </Button>
            </ButtonGroup>
          </RHFForm>
        );
      }}
    </FormProvider>
  );
}
