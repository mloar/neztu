#ifndef NEZTU_DISPATCHER_H
#define NEZTU_DISPATCHER_H
class Dispatcher
{
public:
  Dispatcher();
  void Dispatch(cgicc::FCgiIO&);

  typedef void(*path_handler)(cgicc::FCgiIO&);

private:
  typedef std::map<std::string, path_handler > PathMap;
  PathMap m_paths;
};
#endif
