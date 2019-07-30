import MockAdapter from 'axios-mock-adapter';

export default async (mock: MockAdapter) => {
  const mockData = (await import(
    /* webpackChunkName: "mock-data" */ './mock-data'
  )).default;

  // getMyThemesAndTopics
  mock.onGet('/users/mydetails').reply(_ => {

    const loggedInUserId = window.sessionStorage.getItem('mockLoginUserId');

    if (!loggedInUserId) {
      return [403];
    }

    const matchingUser = mockData.users.find(user => user.id === loggedInUserId)
      || mockData.users[0];

    return [200, Promise.resolve(matchingUser)];

  });
};
