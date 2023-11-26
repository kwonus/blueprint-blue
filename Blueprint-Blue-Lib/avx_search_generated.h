// automatically generated by the FlatBuffers compiler, do not modify


#ifndef FLATBUFFERS_GENERATED_AVXSEARCH_XSEARCHRESULTS_H_
#define FLATBUFFERS_GENERATED_AVXSEARCH_XSEARCHRESULTS_H_

#include "flatbuffers/flatbuffers.h"

// Ensure the included flatbuffers.h is the same version as when this file was
// generated, otherwise it may not be compatible.
static_assert(FLATBUFFERS_VERSION_MAJOR == 23 &&
              FLATBUFFERS_VERSION_MINOR == 5 &&
              FLATBUFFERS_VERSION_REVISION == 26,
             "Non-compatible flatbuffers version included");

namespace XSearchResults {

struct XResults;
struct XResultsBuilder;

struct XFind;
struct XFindBuilder;

struct XFound;
struct XFoundBuilder;

struct XMatch;
struct XMatchBuilder;

enum XResultStatusEnum : int8_t {
  XResultStatusEnum_ERROR = -128,
  XResultStatusEnum_OKAY = 0,
  XResultStatusEnum_WARNING = 1,
  XResultStatusEnum_MIN = XResultStatusEnum_ERROR,
  XResultStatusEnum_MAX = XResultStatusEnum_WARNING
};

inline const XResultStatusEnum (&EnumValuesXResultStatusEnum())[3] {
  static const XResultStatusEnum values[] = {
    XResultStatusEnum_ERROR,
    XResultStatusEnum_OKAY,
    XResultStatusEnum_WARNING
  };
  return values;
}

inline const char *EnumNameXResultStatusEnum(XResultStatusEnum e) {
  switch (e) {
    case XResultStatusEnum_ERROR: return "ERROR";
    case XResultStatusEnum_OKAY: return "OKAY";
    case XResultStatusEnum_WARNING: return "WARNING";
    default: return "";
  }
}

struct XResults FLATBUFFERS_FINAL_CLASS : private ::flatbuffers::Table {
  typedef XResultsBuilder Builder;
  enum FlatBuffersVTableOffset FLATBUFFERS_VTABLE_UNDERLYING_TYPE {
    VT_RESULTS = 4,
    VT_STATUS = 6,
    VT_MESSAGE = 8,
    VT_SCOPE_OT = 10,
    VT_SCOPE_NT = 12
  };
  const ::flatbuffers::Vector<::flatbuffers::Offset<XSearchResults::XFind>> *results() const {
    return GetPointer<const ::flatbuffers::Vector<::flatbuffers::Offset<XSearchResults::XFind>> *>(VT_RESULTS);
  }
  XSearchResults::XResultStatusEnum status() const {
    return static_cast<XSearchResults::XResultStatusEnum>(GetField<int8_t>(VT_STATUS, -128));
  }
  const ::flatbuffers::String *message() const {
    return GetPointer<const ::flatbuffers::String *>(VT_MESSAGE);
  }
  uint64_t scope_ot() const {
    return GetField<uint64_t>(VT_SCOPE_OT, 0);
  }
  uint32_t scope_nt() const {
    return GetField<uint32_t>(VT_SCOPE_NT, 0);
  }
  bool Verify(::flatbuffers::Verifier &verifier) const {
    return VerifyTableStart(verifier) &&
           VerifyOffsetRequired(verifier, VT_RESULTS) &&
           verifier.VerifyVector(results()) &&
           verifier.VerifyVectorOfTables(results()) &&
           VerifyField<int8_t>(verifier, VT_STATUS, 1) &&
           VerifyOffsetRequired(verifier, VT_MESSAGE) &&
           verifier.VerifyString(message()) &&
           VerifyField<uint64_t>(verifier, VT_SCOPE_OT, 8) &&
           VerifyField<uint32_t>(verifier, VT_SCOPE_NT, 4) &&
           verifier.EndTable();
  }
};

struct XResultsBuilder {
  typedef XResults Table;
  ::flatbuffers::FlatBufferBuilder &fbb_;
  ::flatbuffers::uoffset_t start_;
  void add_results(::flatbuffers::Offset<::flatbuffers::Vector<::flatbuffers::Offset<XSearchResults::XFind>>> results) {
    fbb_.AddOffset(XResults::VT_RESULTS, results);
  }
  void add_status(XSearchResults::XResultStatusEnum status) {
    fbb_.AddElement<int8_t>(XResults::VT_STATUS, static_cast<int8_t>(status), -128);
  }
  void add_message(::flatbuffers::Offset<::flatbuffers::String> message) {
    fbb_.AddOffset(XResults::VT_MESSAGE, message);
  }
  void add_scope_ot(uint64_t scope_ot) {
    fbb_.AddElement<uint64_t>(XResults::VT_SCOPE_OT, scope_ot, 0);
  }
  void add_scope_nt(uint32_t scope_nt) {
    fbb_.AddElement<uint32_t>(XResults::VT_SCOPE_NT, scope_nt, 0);
  }
  explicit XResultsBuilder(::flatbuffers::FlatBufferBuilder &_fbb)
        : fbb_(_fbb) {
    start_ = fbb_.StartTable();
  }
  ::flatbuffers::Offset<XResults> Finish() {
    const auto end = fbb_.EndTable(start_);
    auto o = ::flatbuffers::Offset<XResults>(end);
    fbb_.Required(o, XResults::VT_RESULTS);
    fbb_.Required(o, XResults::VT_MESSAGE);
    return o;
  }
};

inline ::flatbuffers::Offset<XResults> CreateXResults(
    ::flatbuffers::FlatBufferBuilder &_fbb,
    ::flatbuffers::Offset<::flatbuffers::Vector<::flatbuffers::Offset<XSearchResults::XFind>>> results = 0,
    XSearchResults::XResultStatusEnum status = XSearchResults::XResultStatusEnum_ERROR,
    ::flatbuffers::Offset<::flatbuffers::String> message = 0,
    uint64_t scope_ot = 0,
    uint32_t scope_nt = 0) {
  XResultsBuilder builder_(_fbb);
  builder_.add_scope_ot(scope_ot);
  builder_.add_scope_nt(scope_nt);
  builder_.add_message(message);
  builder_.add_results(results);
  builder_.add_status(status);
  return builder_.Finish();
}

inline ::flatbuffers::Offset<XResults> CreateXResultsDirect(
    ::flatbuffers::FlatBufferBuilder &_fbb,
    const std::vector<::flatbuffers::Offset<XSearchResults::XFind>> *results = nullptr,
    XSearchResults::XResultStatusEnum status = XSearchResults::XResultStatusEnum_ERROR,
    const char *message = nullptr,
    uint64_t scope_ot = 0,
    uint32_t scope_nt = 0) {
  auto results__ = results ? _fbb.CreateVector<::flatbuffers::Offset<XSearchResults::XFind>>(*results) : 0;
  auto message__ = message ? _fbb.CreateString(message) : 0;
  return XSearchResults::CreateXResults(
      _fbb,
      results__,
      status,
      message__,
      scope_ot,
      scope_nt);
}

struct XFind FLATBUFFERS_FINAL_CLASS : private ::flatbuffers::Table {
  typedef XFindBuilder Builder;
  enum FlatBuffersVTableOffset FLATBUFFERS_VTABLE_UNDERLYING_TYPE {
    VT_FIND = 4,
    VT_FOUND = 6
  };
  const ::flatbuffers::String *find() const {
    return GetPointer<const ::flatbuffers::String *>(VT_FIND);
  }
  const ::flatbuffers::Vector<::flatbuffers::Offset<XSearchResults::XFound>> *found() const {
    return GetPointer<const ::flatbuffers::Vector<::flatbuffers::Offset<XSearchResults::XFound>> *>(VT_FOUND);
  }
  bool Verify(::flatbuffers::Verifier &verifier) const {
    return VerifyTableStart(verifier) &&
           VerifyOffsetRequired(verifier, VT_FIND) &&
           verifier.VerifyString(find()) &&
           VerifyOffset(verifier, VT_FOUND) &&
           verifier.VerifyVector(found()) &&
           verifier.VerifyVectorOfTables(found()) &&
           verifier.EndTable();
  }
};

struct XFindBuilder {
  typedef XFind Table;
  ::flatbuffers::FlatBufferBuilder &fbb_;
  ::flatbuffers::uoffset_t start_;
  void add_find(::flatbuffers::Offset<::flatbuffers::String> find) {
    fbb_.AddOffset(XFind::VT_FIND, find);
  }
  void add_found(::flatbuffers::Offset<::flatbuffers::Vector<::flatbuffers::Offset<XSearchResults::XFound>>> found) {
    fbb_.AddOffset(XFind::VT_FOUND, found);
  }
  explicit XFindBuilder(::flatbuffers::FlatBufferBuilder &_fbb)
        : fbb_(_fbb) {
    start_ = fbb_.StartTable();
  }
  ::flatbuffers::Offset<XFind> Finish() {
    const auto end = fbb_.EndTable(start_);
    auto o = ::flatbuffers::Offset<XFind>(end);
    fbb_.Required(o, XFind::VT_FIND);
    return o;
  }
};

inline ::flatbuffers::Offset<XFind> CreateXFind(
    ::flatbuffers::FlatBufferBuilder &_fbb,
    ::flatbuffers::Offset<::flatbuffers::String> find = 0,
    ::flatbuffers::Offset<::flatbuffers::Vector<::flatbuffers::Offset<XSearchResults::XFound>>> found = 0) {
  XFindBuilder builder_(_fbb);
  builder_.add_found(found);
  builder_.add_find(find);
  return builder_.Finish();
}

inline ::flatbuffers::Offset<XFind> CreateXFindDirect(
    ::flatbuffers::FlatBufferBuilder &_fbb,
    const char *find = nullptr,
    const std::vector<::flatbuffers::Offset<XSearchResults::XFound>> *found = nullptr) {
  auto find__ = find ? _fbb.CreateString(find) : 0;
  auto found__ = found ? _fbb.CreateVector<::flatbuffers::Offset<XSearchResults::XFound>>(*found) : 0;
  return XSearchResults::CreateXFind(
      _fbb,
      find__,
      found__);
}

struct XFound FLATBUFFERS_FINAL_CLASS : private ::flatbuffers::Table {
  typedef XFoundBuilder Builder;
  enum FlatBuffersVTableOffset FLATBUFFERS_VTABLE_UNDERLYING_TYPE {
    VT_START = 4,
    VT_UNTIL = 6,
    VT_MATCHES = 8
  };
  uint32_t start() const {
    return GetField<uint32_t>(VT_START, 0);
  }
  uint32_t until() const {
    return GetField<uint32_t>(VT_UNTIL, 0);
  }
  const ::flatbuffers::Vector<::flatbuffers::Offset<XSearchResults::XMatch>> *matches() const {
    return GetPointer<const ::flatbuffers::Vector<::flatbuffers::Offset<XSearchResults::XMatch>> *>(VT_MATCHES);
  }
  bool Verify(::flatbuffers::Verifier &verifier) const {
    return VerifyTableStart(verifier) &&
           VerifyField<uint32_t>(verifier, VT_START, 4) &&
           VerifyField<uint32_t>(verifier, VT_UNTIL, 4) &&
           VerifyOffsetRequired(verifier, VT_MATCHES) &&
           verifier.VerifyVector(matches()) &&
           verifier.VerifyVectorOfTables(matches()) &&
           verifier.EndTable();
  }
};

struct XFoundBuilder {
  typedef XFound Table;
  ::flatbuffers::FlatBufferBuilder &fbb_;
  ::flatbuffers::uoffset_t start_;
  void add_start(uint32_t start) {
    fbb_.AddElement<uint32_t>(XFound::VT_START, start, 0);
  }
  void add_until(uint32_t until) {
    fbb_.AddElement<uint32_t>(XFound::VT_UNTIL, until, 0);
  }
  void add_matches(::flatbuffers::Offset<::flatbuffers::Vector<::flatbuffers::Offset<XSearchResults::XMatch>>> matches) {
    fbb_.AddOffset(XFound::VT_MATCHES, matches);
  }
  explicit XFoundBuilder(::flatbuffers::FlatBufferBuilder &_fbb)
        : fbb_(_fbb) {
    start_ = fbb_.StartTable();
  }
  ::flatbuffers::Offset<XFound> Finish() {
    const auto end = fbb_.EndTable(start_);
    auto o = ::flatbuffers::Offset<XFound>(end);
    fbb_.Required(o, XFound::VT_MATCHES);
    return o;
  }
};

inline ::flatbuffers::Offset<XFound> CreateXFound(
    ::flatbuffers::FlatBufferBuilder &_fbb,
    uint32_t start = 0,
    uint32_t until = 0,
    ::flatbuffers::Offset<::flatbuffers::Vector<::flatbuffers::Offset<XSearchResults::XMatch>>> matches = 0) {
  XFoundBuilder builder_(_fbb);
  builder_.add_matches(matches);
  builder_.add_until(until);
  builder_.add_start(start);
  return builder_.Finish();
}

inline ::flatbuffers::Offset<XFound> CreateXFoundDirect(
    ::flatbuffers::FlatBufferBuilder &_fbb,
    uint32_t start = 0,
    uint32_t until = 0,
    const std::vector<::flatbuffers::Offset<XSearchResults::XMatch>> *matches = nullptr) {
  auto matches__ = matches ? _fbb.CreateVector<::flatbuffers::Offset<XSearchResults::XMatch>>(*matches) : 0;
  return XSearchResults::CreateXFound(
      _fbb,
      start,
      until,
      matches__);
}

struct XMatch FLATBUFFERS_FINAL_CLASS : private ::flatbuffers::Table {
  typedef XMatchBuilder Builder;
  enum FlatBuffersVTableOffset FLATBUFFERS_VTABLE_UNDERLYING_TYPE {
    VT_FRAGMENT = 4,
    VT_FEATURE = 6,
    VT_COORDINATES = 8
  };
  const ::flatbuffers::String *fragment() const {
    return GetPointer<const ::flatbuffers::String *>(VT_FRAGMENT);
  }
  const ::flatbuffers::String *feature() const {
    return GetPointer<const ::flatbuffers::String *>(VT_FEATURE);
  }
  uint32_t coordinates() const {
    return GetField<uint32_t>(VT_COORDINATES, 0);
  }
  bool Verify(::flatbuffers::Verifier &verifier) const {
    return VerifyTableStart(verifier) &&
           VerifyOffsetRequired(verifier, VT_FRAGMENT) &&
           verifier.VerifyString(fragment()) &&
           VerifyOffsetRequired(verifier, VT_FEATURE) &&
           verifier.VerifyString(feature()) &&
           VerifyField<uint32_t>(verifier, VT_COORDINATES, 4) &&
           verifier.EndTable();
  }
};

struct XMatchBuilder {
  typedef XMatch Table;
  ::flatbuffers::FlatBufferBuilder &fbb_;
  ::flatbuffers::uoffset_t start_;
  void add_fragment(::flatbuffers::Offset<::flatbuffers::String> fragment) {
    fbb_.AddOffset(XMatch::VT_FRAGMENT, fragment);
  }
  void add_feature(::flatbuffers::Offset<::flatbuffers::String> feature) {
    fbb_.AddOffset(XMatch::VT_FEATURE, feature);
  }
  void add_coordinates(uint32_t coordinates) {
    fbb_.AddElement<uint32_t>(XMatch::VT_COORDINATES, coordinates, 0);
  }
  explicit XMatchBuilder(::flatbuffers::FlatBufferBuilder &_fbb)
        : fbb_(_fbb) {
    start_ = fbb_.StartTable();
  }
  ::flatbuffers::Offset<XMatch> Finish() {
    const auto end = fbb_.EndTable(start_);
    auto o = ::flatbuffers::Offset<XMatch>(end);
    fbb_.Required(o, XMatch::VT_FRAGMENT);
    fbb_.Required(o, XMatch::VT_FEATURE);
    return o;
  }
};

inline ::flatbuffers::Offset<XMatch> CreateXMatch(
    ::flatbuffers::FlatBufferBuilder &_fbb,
    ::flatbuffers::Offset<::flatbuffers::String> fragment = 0,
    ::flatbuffers::Offset<::flatbuffers::String> feature = 0,
    uint32_t coordinates = 0) {
  XMatchBuilder builder_(_fbb);
  builder_.add_coordinates(coordinates);
  builder_.add_feature(feature);
  builder_.add_fragment(fragment);
  return builder_.Finish();
}

inline ::flatbuffers::Offset<XMatch> CreateXMatchDirect(
    ::flatbuffers::FlatBufferBuilder &_fbb,
    const char *fragment = nullptr,
    const char *feature = nullptr,
    uint32_t coordinates = 0) {
  auto fragment__ = fragment ? _fbb.CreateString(fragment) : 0;
  auto feature__ = feature ? _fbb.CreateString(feature) : 0;
  return XSearchResults::CreateXMatch(
      _fbb,
      fragment__,
      feature__,
      coordinates);
}

inline const XSearchResults::XResults *GetXResults(const void *buf) {
  return ::flatbuffers::GetRoot<XSearchResults::XResults>(buf);
}

inline const XSearchResults::XResults *GetSizePrefixedXResults(const void *buf) {
  return ::flatbuffers::GetSizePrefixedRoot<XSearchResults::XResults>(buf);
}

inline bool VerifyXResultsBuffer(
    ::flatbuffers::Verifier &verifier) {
  return verifier.VerifyBuffer<XSearchResults::XResults>(nullptr);
}

inline bool VerifySizePrefixedXResultsBuffer(
    ::flatbuffers::Verifier &verifier) {
  return verifier.VerifySizePrefixedBuffer<XSearchResults::XResults>(nullptr);
}

inline void FinishXResultsBuffer(
    ::flatbuffers::FlatBufferBuilder &fbb,
    ::flatbuffers::Offset<XSearchResults::XResults> root) {
  fbb.Finish(root);
}

inline void FinishSizePrefixedXResultsBuffer(
    ::flatbuffers::FlatBufferBuilder &fbb,
    ::flatbuffers::Offset<XSearchResults::XResults> root) {
  fbb.FinishSizePrefixed(root);
}

}  // namespace XSearchResults

#endif  // FLATBUFFERS_GENERATED_AVXSEARCH_XSEARCHRESULTS_H_
