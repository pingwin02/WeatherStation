import React from "react";
import { BrowserRouter as Router, Route, Routes } from "react-router-dom";
import Dashboard from "./components/Dashboard";
import SensorDetails from "./components/SensorDetails";
import TokenBalance from "./components/TokenBalance";
import NavMenu from "./components/NavMenu";
import SensorDataTable from "./components/SensorDataTable";

function App() {
  return (
    <Router>
      <div>
        <NavMenu />
        <Routes>
          <Route path="/" element={<Dashboard />} />
          <Route path="/sensors/:id" element={<SensorDetails />} />
          <Route path="/sensors/data" element={<SensorDataTable />} />
          <Route path="/tokens" element={<TokenBalance />} />
        </Routes>
      </div>
    </Router>
  );
}

export default App;
