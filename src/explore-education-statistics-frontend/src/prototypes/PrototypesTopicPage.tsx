import React from 'react';
// import CollapsibleSection from '../components/CollapsibleSection';
// import Details from '../components/Details';
// import PrototypeDataSample from './components/PrototypeDataSample';
import PrototypePage from './components/PrototypePage';

const TopicPage = () => {
  return (
    <PrototypePage breadcrumbs={[{ text: 'Schools' }]}>
      This is the topic page
      <a href="theme">Theme page</a>
    </PrototypePage>
  );
};

export default TopicPage;
