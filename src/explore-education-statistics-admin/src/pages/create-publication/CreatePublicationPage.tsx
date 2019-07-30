import React from 'react';
import {RouteComponentProps} from 'react-router';

interface MatchProps {
  topicId: string;
}

const CreatePublicationPage = ({ match }: RouteComponentProps<MatchProps>) => {
  const { topicId } = match.params;

  return (
    <span>{topicId}</span>
  );
};

export default CreatePublicationPage;
