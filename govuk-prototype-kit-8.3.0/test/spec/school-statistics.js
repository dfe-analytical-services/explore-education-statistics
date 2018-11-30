/* eslint-env mocha */
var request = require('supertest')
var app = require('../../server.js')

/**
 * Basic sanity checks on the dev server
 */
describe('The School stastics Page', function () {
  it('should send with a well formed response for school stastics', function (done) {
    request(app)
      .get('/school-statistics/')
      .expect('Content-Type', /text\/html/)
      .expect(200)
      .end(function (err, res) {
        if (err) {
          done(err)
        } else {
          done()
        }
      })
  })
})
describe('The School stastics Absence Page', function () {
  it('should send with a well formed response for school stastics Pupil absence', function (done) {
    request(app)
      .get('/school-statistics/pupil-absence/')
      .expect('Content-Type', /text\/html/)
      .expect(200)
      .end(function (err, res) {
        if (err) {
          done(err)
        } else {
          done()
        }
      })
  })
})
describe('The Pupil absence Page', function () {
  it('should send with a well formed response for Pupil absence in schools in England', function (done) {
    request(app)
      .get('/school-statistics/pupil-absence/pupil-absence-in-schools-in-england/')
      .expect('Content-Type', /text\/html/)
      .expect(200)
      .end(function (err, res) {
        if (err) {
          done(err)
        } else {
          done()
        }
      })
  })
})
