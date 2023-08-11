import { useConfig } from '@admin/contexts/ConfigContext';
import glossaryQueries from '@admin/queries/glossaryQueries';
import { GlossaryItem } from '@admin/types/ckeditor';
import Button from '@common/components/Button';
import FormComboBox from '@common/components/form/FormComboBox';
import ButtonGroup from '@common/components/ButtonGroup';
import { Form, FormFieldTextInput } from '@common/components/form';
import useDebouncedCallback from '@common/hooks/useDebouncedCallback';
import { GlossaryEntry } from '@common/services/types/glossary';
import Yup from '@common/validation/yup';
import React, { useMemo, useState } from 'react';
import { Formik } from 'formik';
import { useQuery } from '@tanstack/react-query';
import LoadingSpinner from '@common/components/LoadingSpinner';

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

  const allGlossaryEntries = useMemo(() => glossary.flatMap(g => g.entries), [
    glossary,
  ]);

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

  return (
    <LoadingSpinner loading={isLoading} hideText text="Loading glossary">
      <Formik<FormValues>
        initialValues={{
          slug: '',
          text: '',
        }}
        validationSchema={Yup.object({
          slug: Yup.string().required('Select a glossary entry'),
          text: Yup.string().when('slug', {
            is: (val: string) => val,
            then: s => s.required('Enter link text'),
          }),
        })}
        onSubmit={values =>
          onSubmit({
            text: values.text,
            url: `${config.PublicAppUrl}/glossary#${values.slug}`,
          })
        }
      >
        {form => (
          <Form id="glossaryForm">
            <FormComboBox
              hideSearchIcon={!!selectedEntry}
              id="glossarySearch"
              inputLabel="Glossary entry"
              options={searchResults.map(result => result.title)}
              inputValue={selectedEntry?.title}
              onInputChange={event => {
                search(event.target.value);
              }}
              onSelect={selectedIndex => {
                form.setFieldValue('text', searchResults[selectedIndex].title);
                form.setFieldValue('slug', searchResults[selectedIndex].slug);
                setSelectedEntry(searchResults[selectedIndex]);
              }}
            />
            {form.values.slug && (
              <>
                <p className="govuk-!-margin-top-1">Slug: {form.values.slug}</p>

                <FormFieldTextInput
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
                  form.submitForm();
                }}
              >
                Insert
              </Button>
              <Button variant="secondary" onClick={onCancel}>
                Cancel
              </Button>
            </ButtonGroup>
          </Form>
        )}
      </Formik>
    </LoadingSpinner>
  );
}
