import React from 'react';
import { BrowserRouter as Router, Route, Routes } from 'react-router-dom';
import SensorList from './components/SensorList';
import SensorDetails from './components/SensorDetails';
import TokenBalance from './components/TokenBalance';

function App() {
  return (
      <Router>
        <Routes>
            <Route path="/" element={<SensorList />} />
            <Route path="/sensors/:id" element={<SensorDetails />} />
            <Route path="/tokens" element={<TokenBalance />} />
        </Routes>
      </Router>
  );
}

export default App;
