import FormRadioGroup from '@common/components/form/FormRadioGroup';
import React from 'react';
import MenuDetails from './MenuDetails';

export type MenuChangeEventHandler = (values: {
  publicationId: string;
  publicationName: string;
}) => void;

interface Props {
  onChange: MenuChangeEventHandler;
  options: {
    id: string;
    name: string;
    topics: {
      id: string;
      name: string;
      publications: {
        id: string;
        name: string;
      }[];
    }[];
  }[];
  value: string;
}

const PublicationMenu = ({ options, onChange, value }: Props) => {
  return (
    <>
      {options.map(option => (
        <MenuDetails summary={option.name} key={option.id}>
          {option.topics.map(topic => (
            <MenuDetails summary={topic.name} key={topic.id}>
              <FormRadioGroup
                value={value}
                name="publicationId"
                id={topic.id}
                onChange={event => {
                  const publication = topic.publications.find(
                    item => item.id === event.target.value,
                  );

                  if (publication) {
                    onChange({
                      publicationId: publication.id,
                      publicationName: publication.name,
                    });
                  }
                }}
                options={topic.publications.map(publication => ({
                  id: publication.id,
                  label: publication.name,
                  value: publication.id,
                }))}
              />
            </MenuDetails>
          ))}
        </MenuDetails>
      ))}
    </>
  );
};

export default PublicationMenu;
